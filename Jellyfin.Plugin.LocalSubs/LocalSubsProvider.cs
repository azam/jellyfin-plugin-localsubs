using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.LocalSubs;

/// <summary>
/// Local file subtitle provider.
/// </summary>
public class LocalSubsProvider : ISubtitleProvider
{
    private readonly ILogger<LocalSubsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSubsProvider"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger{LocalSubsProvider}"/> interface.</param>
    /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> for creating Http Clients.</param>
    public LocalSubsProvider(ILogger<LocalSubsProvider> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string Name => LocalSubsConstants.PLUGINNAME;

    /// <inheritdoc/>
    public IEnumerable<VideoContentType> SupportedMediaTypes => LocalSubsConstants.MEDIATYPES;

    /// <inheritdoc/>
    public Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetSubtitles id: {Id}", id);
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Missing param", nameof(id));
        }

        string[] parts = id.Split(LocalSubsConstants.IDSEPARATOR);
        if (parts.Length < 3)
        {
            throw new ArgumentException("Invalid id", nameof(id));
        }

        string ext = parts[0];
        string lang = parts[1];
        if (string.IsNullOrEmpty(ext) || string.IsNullOrEmpty(lang))
        {
            throw new ArgumentException("Invalid id (extension and/or language is invalid)", nameof(id));
        }

        string path = id.Substring(ext.Length + lang.Length + 2);
        if (!File.Exists(path))
        {
            throw new ArgumentException("File do not exist", nameof(id));
        }

        _logger.LogInformation("GetSubtitles return file: {Path}", path);
        return Task.FromResult(new SubtitleResponse
        {
            Format = ext,
            Language = lang,
            Stream = File.OpenRead(path),
        });
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:OpeningParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Occurring on delegate syntax.")]
    private static string GeneratePattern(string text, IDictionary<string, string> dict)
    {
        return "^" + Regex.Replace(text, "(" + string.Join("|", dict.Keys) + ")", delegate (Match m)
        {
            return dict[m.Value];
        }) + "$";
    }

    private IEnumerable<string> MatchFile(string mediaDir, string template, IDictionary<string, string> placeholders)
    {
        string[] parts = template.Split(Path.DirectorySeparatorChar);
        _logger.LogDebug("MatchFile using parts: {Parts} separator: {DirectorySeparatorChar}", parts, Path.DirectorySeparatorChar);
        if (parts.Length < 1)
        {
            return Enumerable.Empty<string>();
        }

        List<string> dirs = [mediaDir];
        for (int i = 0; i < parts.Length - 1; i++)
        {
            string dirPattern = GeneratePattern(parts[i], placeholders);
            Regex dirRegex = new Regex(dirPattern);
            List<string> subDirs = [];
            foreach (string dir in dirs)
            {
                _logger.LogDebug("Finding directories in {Directory} using pattern {DirectoryPattern}", dir, dirPattern);
                subDirs.AddRange(Directory.EnumerateDirectories(dir).Where(d => dirRegex.IsMatch(Path.GetFileName(d) ?? string.Empty) && !subDirs.Contains(d)));
            }

            dirs.Clear();
            dirs.AddRange(subDirs);
        }

        List<string> files = [];
        string filePattern = GeneratePattern(parts[parts.Length - 1], placeholders);
        Regex fileRegex = new Regex(filePattern);
        foreach (string dir in dirs)
        {
            _logger.LogDebug("Finding files in {Directory} using pattern {FilePattern}", dir, filePattern);
            files.AddRange(Directory.EnumerateFiles(dir).Where(f => fileRegex.IsMatch(Path.GetFileName(f)) && !files.Contains(f)));
        }

        return files;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Search MediaPath: {MediaPath} TwoLetterISOLanguageName: {TwoLetterISOLanguageName} Language: {Language}", request.MediaPath, request.TwoLetterISOLanguageName, request.Language);

        string[] templates = LocalSubsPlugin.Instance!.Configuration.Templates;

        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(request.MediaPath) || templates == null || templates.Length < 1)
        {
            return Task.FromResult(Enumerable.Empty<RemoteSubtitleInfo>());
        }

        List<string> langStrings =
        [
            request.Language, // Three letter language code
            request.TwoLetterISOLanguageName, // Two letter language code
        ];
        try
        {
            langStrings.Add(CultureInfo.GetCultureInfo(request.TwoLetterISOLanguageName).EnglishName);
        }
        catch (CultureNotFoundException e)
        {
            _logger.LogError(e, "Culture not found for {TwoLetterISOLanguageName}", request.TwoLetterISOLanguageName);
        }

        string dir = Path.GetDirectoryName(request.MediaPath) ?? string.Empty;
        string f = Path.GetFileName(request.MediaPath);
        string fn = Path.GetFileNameWithoutExtension(request.MediaPath);
        string fe = Path.GetExtension(request.MediaPath);
        Dictionary<string, string> placeholders = new Dictionary<string, string>
        {
            { "%f%", Regex.Escape(f) },
            { "%fn%", Regex.Escape(fn) },
            { "%fe%", Regex.Escape(fe) },
            { "%n%", "[0-9]+" },
            { "%l%", "(?i)(" + string.Join("|", langStrings) + ")" },
            { "%any%", ".+" }
        };
        List<RemoteSubtitleInfo> matches = [];
        foreach (string template in templates)
        {
            if (string.IsNullOrEmpty(template))
            {
                continue;
            }

            foreach (string match in MatchFile(dir, template, placeholders))
            {
                string ext = (Path.GetExtension(match) ?? "srt").ToLowerInvariant().Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase);
                string lang = request.Language;
                string id = string.Join(LocalSubsConstants.IDSEPARATOR, ext, lang, match);
                _logger.LogInformation("RemoteSubtitleInfo MediaPath: {MediaPath} Format: {Extension} Language: {Language} Id: {Id}", request.MediaPath, ext, lang, id);
                matches.Add(new RemoteSubtitleInfo
                {
                    Id = id,
                    ProviderName = LocalSubsConstants.PLUGINNAME,
                    Format = ext,
                    ThreeLetterISOLanguageName = lang,
                    DateCreated = new FileInfo(match).CreationTime,
                });
            }
        }

        return Task.FromResult(matches.AsEnumerable());
    }
}
