using System.Collections.Generic;
using Jellyfin.Plugin.DuplicateFinder.Model;

namespace Jellyfin.Plugin.DuplicateFinder.Services;

// Scores one version. Higher is better. Points are explained in Reasons so the
// recommendation can be checked by the user. Nothing here changes files.
public static class Scoring
{
    public static void Apply(MediaVersion v, bool preferDubbed)
    {
        var reasons = new List<string>();
        var score = 0;

        // Resolution
        var res = v.Height >= 2160 ? 40 : v.Height >= 1080 ? 30 : v.Height >= 720 ? 15 : 5;
        score += res;
        reasons.Add((v.Height > 0 ? v.Height + "p" : "unknown resolution") + " (+" + res + ")");

        // Source (from the file name)
        var src = v.Source switch
        {
            "Remux" => 15,
            "BluRay" => 12,
            "WEB-DL" => 10,
            "WEBRip" => 7,
            "HDTV" => 4,
            "DVDRip" => 3,
            _ => 0
        };
        score += src;
        if (src > 0) reasons.Add(v.Source + " (+" + src + ")");

        // Codec: newer codecs give the same quality at a smaller size
        var codec = v.VideoCodec.ToLowerInvariant();
        var codecPts = codec is "hevc" or "h265" or "av1" ? 6 : codec == "h264" ? 3 : 0;
        score += codecPts;
        if (codecPts > 0) reasons.Add(v.VideoCodec + " (+" + codecPts + ")");

        // Container
        var containerPts = v.Container == "mkv" ? 3 : v.Container == "mp4" ? 2 : 0;
        score += containerPts;
        if (containerPts > 0) reasons.Add(v.Container + " (+" + containerPts + ")");

        // Dubbed audio, following the user's preference
        if (v.IsDubbed)
        {
            if (preferDubbed)
            {
                score += 12;
                reasons.Add("dubbed audio (+12)");
            }
            else
            {
                score -= 4;
                reasons.Add("dubbed audio, not preferred (-4)");
            }
        }

        // Hard subtitles are burned in and cannot be turned off
        if (v.HasHardSub)
        {
            score -= 8;
            reasons.Add("hardcoded subtitles (-8)");
        }

        // Extra subtitle tracks are a small plus, capped
        var subPts = System.Math.Min(v.SubtitleTracks, 3);
        score += subPts;
        if (subPts > 0) reasons.Add(v.SubtitleTracks + " subtitle track(s) (+" + subPts + ")");

        v.Score = score;
        v.Reasons = reasons;
    }
}
