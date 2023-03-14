using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.LocalSubs;

/// <summary>
/// Jellyfin local subtitles plugin.
/// </summary>
public class LocalSubsPlugin : BasePlugin<LocalSubsConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSubsPlugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public LocalSubsPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => LocalSubsConstants.NAME;

    /// <inheritdoc />
    public override Guid Id => Guid.Parse(LocalSubsConstants.GUID);

    /// <summary>Gets current plugin instance.</summary>
    public static LocalSubsPlugin? Instance { get; private set; }

    /// <summary>Gets plugin configuration.</summary>
    public LocalSubsConfiguration LocalSubsConfiguration => Configuration;

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "LocalSubsPage",
                EmbeddedResourcePath = GetType().Namespace + ".LocalSubsPage.html",
            },
            new PluginPageInfo
            {
                Name = "LocalSubsPageScript",
                EmbeddedResourcePath = GetType().Namespace + ".LocalSubsPage.js",
            },
        };
    }
}
