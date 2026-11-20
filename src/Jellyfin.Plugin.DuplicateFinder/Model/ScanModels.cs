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

public class MediaVersion
{
    public string ItemId { get; set; } = "";
    public string Path { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = "";
    public string Container { get; set; } = "";
    public string VideoCodec { get; set; } = "";
    public int Height { get; set; }
    public string Source { get; set; } = "";
    public bool IsDubbed { get; set; }
    public bool HasHardSub { get; set; }
    public int AudioTracks { get; set; }
    public int SubtitleTracks { get; set; }
    public int Score { get; set; }
    // keep | remove | review | equal (identical copies, user chooses)
    public string Recommendation { get; set; } = "review";
    public List<string> Reasons { get; set; } = new();
}

public class VersionGroup
{
    public string Title { get; set; } = "";
    public int? Year { get; set; }
    // identical (same specs and same SHA-256) | same-spec (same specs, different content) | version
    public string Kind { get; set; } = "version";
    // high (TMDb ID) | low (title and year only)
    public string Confidence { get; set; } = "high";
    public List<MediaVersion> Versions { get; set; } = new();
}

public class ScanJob
{
    public string State { get; set; } = "idle";
    public DateTime? Started { get; set; }
    public DateTime? Finished { get; set; }
    public string? Error { get; set; }
    public List<VersionGroup> Groups { get; set; } = new();
}
