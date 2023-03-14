using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.LocalSubs;

/// <summary>Constant values.</summary>
public static class LocalSubsConstants
{
    /// <summary>Global UID.</summary>
    public const string GUID = "7de4aa03-f418-4e1c-a8ba-08ccecba4ab5";

    /// <summary>Display name.</summary>
    public const string NAME = "Local Subs";

    /// <summary>SubtitleInfo ID section separator.</summary>
    public const char IDSEPARATOR = '-';

    /// <summary>Supported media types.</summary>
    public static readonly VideoContentType[] MEDIATYPES = new[] { VideoContentType.Episode, VideoContentType.Movie };
}
