using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DuplicateFinder.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    // Minimum title similarity (0-1) for "similar" groups.
    public double SimilarityThreshold { get; set; } = 0.9;

    // Whether to report similar (not exact) titles in the results.
    public bool IncludeSimilarTitles { get; set; } = true;
}
