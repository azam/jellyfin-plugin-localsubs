using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.LocalSubs;

/// <summary>Plugin configuration.</summary>
[Serializable]
public class LocalSubsConfiguration : BasePluginConfiguration
{
    private List<string> _templates;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSubsConfiguration"/> class.
    /// </summary>
    public LocalSubsConfiguration()
    {
        _templates = [];
    }

    /// <summary>Gets or sets template strings.</summary>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "CA1819", Justification = "Backed by a list object.")]
    public string[] Templates
    {
        get
        {
            return _templates.ToArray();
        }

        set
        {
            if (value != null)
            {
                _templates.Clear();
                foreach (string template in value)
                {
                    if (!string.IsNullOrEmpty(template))
                    {
                        _templates.Add(template);
                    }
                }
            }
        }
    }

    /// <summary>Adds a new template.</summary>
    /// <param name="template">Template string.</param>
    public void AddTemplate(string template)
    {
        if (!_templates.Contains(template))
        {
            _templates.Add(template);
        }
    }

    /// <summary>Remove a template string.</summary>
    /// <param name="template">Template string.</param>
    public void RemoveTemplate(string template)
    {
        _templates.Remove(template);
    }

    /// <summary>Remove all templates strings.</summary>
    public void ClearTemplates()
    {
        _templates.Clear();
    }

    /// <summary>Reset templates to default.</summary>
    public void ResetTemplates()
    {
        _templates.Clear();
        _templates.Add(Path.Join("Subs", "%fn%", "%n%_%l%.srt"));
        _templates.Add(Path.Join("Subs", "%fn%.%l%.srt"));
        _templates.Add(Path.Join("Subs", "%n%_%l%.srt"));
        _templates.Add(Path.Join("Subs", "%l%.srt"));
        _templates.Add(Path.Join("Subs", "%fn%.srt"));
    }
}
