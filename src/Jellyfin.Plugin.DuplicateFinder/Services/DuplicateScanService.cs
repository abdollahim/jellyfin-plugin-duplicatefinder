using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.DuplicateFinder.Model;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace Jellyfin.Plugin.DuplicateFinder.Services;

public class DuplicateScanService
{
    private static readonly Regex DubRe = new(@"dub(bed)?|farsi|persian", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HardSubRe = new(@"hard\s?t?sub", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ILibraryManager _library;
    private readonly object _gate = new();

    public DuplicateScanService(ILibraryManager library) => _library = library;

    public ScanJob Job { get; private set; } = new();

    private List<BaseItem> AllMovies() => _library.GetItemList(new InternalItemsQuery
    {
        IncludeItemTypes = new[] { BaseItemKind.Movie },
        Recursive = true
    }).ToList();

    public ScanStats GetStats()
    {
        var movies = AllMovies();
        var stats = new ScanStats { TotalMovies = movies.Count };

        foreach (var m in movies)
        {
            var reason = Classifier.SkipReason(m);
            if (reason is not null)
            {
                stats.Skipped.TryGetValue(reason, out var n);
                stats.Skipped[reason] = n + 1;
                if (stats.SkippedSamples.Count < 20)
                {
                    stats.SkippedSamples.Add($"{reason}: {m.Path}");
                }

                continue;
            }

            stats.Eligible++;
            if (!string.IsNullOrEmpty(TmdbOf(m))) stats.WithTmdb++;
            else stats.WithoutTmdb++;
            if (Classifier.IsMultiPart(m)) stats.MultiPart++;
        }

        return stats;
    }

    public void StartScan()
    {
        lock (_gate)
        {
            if (Job.State == "running")
            {
                return;
            }

            Job = new ScanJob { State = "running", Started = DateTime.UtcNow };
        }

        Task.Run(Run);
    }

    private void Run()
    {
        try
        {
            var prefer = Plugin.Instance?.Configuration.PreferDubbed ?? true;
            var eligible = AllMovies().Where(m => Classifier.SkipReason(m) is null).ToList();
            var groups = new List<VersionGroup>();

            // 1. Same TMDb ID, split by part number so Part1 and Part2 stay separate.
            var byTmdb = eligible
                .Where(m => !string.IsNullOrEmpty(TmdbOf(m)))
                .GroupBy(m => TmdbOf(m) + "|part" + Classifier.PartNumber(m))
                .Where(g => g.Count() > 1);

            foreach (var g in byTmdb)
            {
                groups.Add(BuildGroup(g.ToList(), "high", prefer));
            }

            // 2. No TMDb ID: same normalized title, year and part. Lower confidence.
            var byTitle = eligible
                .Where(m => string.IsNullOrEmpty(TmdbOf(m)) && m.ProductionYear.HasValue)
                .GroupBy(m => NormalizeTitle(m.Name) + "|" + m.ProductionYear + "|part" + Classifier.PartNumber(m))
                .Where(g => g.Count() > 1);

            foreach (var g in byTitle)
            {
                groups.Add(BuildGroup(g.ToList(), "low", prefer));
            }

            Job = new ScanJob
            {
                State = "done",
                Started = Job.Started,
                Finished = DateTime.UtcNow,
                Groups = groups.OrderBy(g => g.Title).ToList()
            };
        }
        catch (Exception ex)
        {
            Job = new ScanJob { State = "failed", Error = ex.Message };
        }
    }

    private static VersionGroup BuildGroup(List<BaseItem> items, string confidence, bool prefer)
    {
        var first = items[0];
        var versions = items.Select(m => ToVersion(m, prefer)).ToList();
        foreach (var v in versions)
        {
            Scoring.Apply(v, prefer);
        }

        var sameSpecs = versions.Select(SpecKey).Distinct().Count() == 1;
        var sameSize = versions.Select(v => v.SizeBytes).Distinct().Count() == 1;

        // Hash only when specs and size already match. Hashing is slow, so skip the rest.
        var kind = "version";
        if (sameSpecs && sameSize)
        {
            foreach (var v in versions)
            {
                v.Sha256 = HashOf(v.Path);
            }

            var identical = versions[0].Sha256 != string.Empty
                && versions.Select(v => v.Sha256).Distinct().Count() == 1;
            kind = identical ? "identical" : "same-spec";
        }

        if (kind == "identical")
        {
            // Same content: no keep/remove. The user chooses which copy to keep.
            foreach (var v in versions)
            {
                v.Recommendation = "equal";
            }

            versions = versions.OrderBy(v => v.Path).ToList();
        }
        else if (kind == "same-spec")
        {
            // Same specs but different content: the user decides.
            foreach (var v in versions)
            {
                v.Recommendation = "review";
            }

            versions = versions.OrderByDescending(v => v.Score).ThenByDescending(v => v.SizeBytes).ToList();
        }
        else
        {
            versions = versions.OrderByDescending(v => v.Score).ThenByDescending(v => v.SizeBytes).ToList();
            var best = versions[0];
            best.Recommendation = "keep";
            foreach (var v in versions.Skip(1))
            {
                // Close scores are not decisive, so ask the user instead of recommending removal.
                v.Recommendation = best.Score - v.Score >= 5 ? "remove" : "review";
            }
        }

        return new VersionGroup
        {
            Title = first.Name,
            Year = first.ProductionYear,
            Kind = kind,
            Confidence = confidence,
            Versions = versions
        };
    }

    // Everything that describes what the user sees. Same key means same specs.
    private static string SpecKey(MediaVersion v) =>
        $"{v.Height}|{v.VideoCodec}|{v.Source}|{v.IsDubbed}|{v.HasHardSub}|{v.SubtitleTracks}|{v.AudioTracks}|{v.Container}";

    private static string HashOf(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            using var fs = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(fs));
        }
        catch (IOException)
        {
            return string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }

    private static MediaVersion ToVersion(BaseItem m, bool prefer)
    {
        var path = m.Path ?? string.Empty;
        var name = Path.GetFileName(path);
        var streams = m.GetMediaStreams();
        var video = streams.FirstOrDefault(s => s.Type == MediaStreamType.Video);
        var audio = streams.Where(s => s.Type == MediaStreamType.Audio).ToList();
        var subs = streams.Where(s => s.Type == MediaStreamType.Subtitle).ToList();

        // Two or more distinct audio languages also counts as dubbed.
        var audioLanguages = audio.Select(a => a.Language ?? string.Empty)
            .Where(l => l.Length > 0)
            .Distinct()
            .Count();

        return new MediaVersion
        {
            ItemId = m.Id.ToString("N"),
            Path = path,
            SizeBytes = SizeOf(path),
            Container = Path.GetExtension(path).TrimStart('.').ToLowerInvariant(),
            VideoCodec = video?.Codec ?? string.Empty,
            Height = video?.Height ?? 0,
            Source = DetectSource(name),
            IsDubbed = DubRe.IsMatch(name) || audioLanguages >= 2,
            HasHardSub = HardSubRe.IsMatch(name),
            AudioTracks = audio.Count,
            SubtitleTracks = subs.Count
        };
    }

    private static string DetectSource(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("remux")) return "Remux";
        if (n.Contains("bluray") || n.Contains("blu-ray") || n.Contains("brrip") || n.Contains("bdrip")) return "BluRay";
        if (n.Contains("web-dl") || n.Contains("webdl") || n.Contains("web.dl")) return "WEB-DL";
        if (n.Contains("webrip") || n.Contains("web-rip")) return "WEBRip";
        if (n.Contains("hdtv")) return "HDTV";
        if (n.Contains("dvdrip")) return "DVDRip";
        return string.Empty;
    }

    private static string TmdbOf(BaseItem m) => m.GetProviderId(MetadataProvider.Tmdb) ?? string.Empty;

    private static string NormalizeTitle(string? s)
    {
        var cleaned = new string((s ?? string.Empty).ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray());
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (words.Count > 0 && (words[0] == "the" || words[0] == "a" || words[0] == "an"))
        {
            words.RemoveAt(0);
        }

        return string.Join(' ', words);
    }

    private static long SizeOf(string? path) =>
        !string.IsNullOrEmpty(path) && File.Exists(path) ? new FileInfo(path).Length : 0;
}
