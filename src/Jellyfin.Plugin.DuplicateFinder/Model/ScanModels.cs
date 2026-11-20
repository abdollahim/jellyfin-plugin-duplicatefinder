using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.DuplicateFinder.Model;

public class ScanStats
{
    public int TotalMovies { get; set; }
    public int Eligible { get; set; }
    public int WithTmdb { get; set; }
    public int WithoutTmdb { get; set; }
    public int MultiPart { get; set; }
    public Dictionary<string, int> Skipped { get; set; } = new();
    public List<string> SkippedSamples { get; set; } = new();
}

public record FileEntry(string Name, int? Year, string? Path, long SizeBytes);

public class ScanJob
{
    public string State { get; set; } = "idle";
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public string? Error { get; set; }
    public List<List<FileEntry>> ExactGroups { get; set; } = new();
}
