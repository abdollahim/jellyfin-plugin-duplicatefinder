using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Data.Enums;
using System.Threading.Tasks;
using Jellyfin.Plugin.DuplicateFinder.Model;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace Jellyfin.Plugin.DuplicateFinder.Services;

public class DuplicateScanService
{
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
            if (!string.IsNullOrEmpty(m.GetProviderId(MetadataProvider.Tmdb))) stats.WithTmdb++;
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
            var eligible = AllMovies().Where(m => Classifier.SkipReason(m) is null).ToList();

            var exact = eligible
                .Where(m => !string.IsNullOrEmpty(m.GetProviderId(MetadataProvider.Tmdb)))
                .GroupBy(m => m.GetProviderId(MetadataProvider.Tmdb))
                .Where(g => g.Count() > 1)
                .Select(g => g.Select(m => new FileEntry(m.Name, m.ProductionYear, m.Path, SizeOf(m.Path))).ToList())
                .ToList();

            Job = new ScanJob
            {
                State = "done",
                Started = Job.Started,
                Finished = DateTime.UtcNow,
                ExactGroups = exact
            };
        }
        catch (Exception ex)
        {
            Job = new ScanJob { State = "failed", Error = ex.Message };
        }
    }

    private static long SizeOf(string? path) =>
        !string.IsNullOrEmpty(path) && File.Exists(path) ? new FileInfo(path).Length : 0;
}
