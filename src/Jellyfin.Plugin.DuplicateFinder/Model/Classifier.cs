using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.DuplicateFinder.Model;

public static class Classifier
{
    private static readonly Regex EpisodeRe = new(@"S\d{1,2}E\d{1,3}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PartRe = new(@"(?<![a-z])part\s*\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Returns the reason an item is skipped, or null when it is checked.
    public static string? SkipReason(BaseItem item)
    {
        var path = item.Path ?? string.Empty;
        if (EpisodeRe.IsMatch(path))
        {
            return "TV episode";
        }

        if (item.ProductionYear is null && string.IsNullOrEmpty(item.GetProviderId(MetadataProvider.Tmdb)))
        {
            return "No year and no TMDb ID";
        }

        return null;
    }

    public static bool IsMultiPart(BaseItem item) => PartRe.IsMatch(item.Path ?? string.Empty);
}
