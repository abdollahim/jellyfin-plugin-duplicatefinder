# Duplicate Finder for Jellyfin

A Jellyfin plugin that finds duplicate and similar movies in your library.
**Read-only:** it reports what it finds and never deletes or changes files.

## Features

- Reads the movie library through Jellyfin, so it works on what Jellyfin has indexed.
- Groups movies by TMDb ID, and by normalized title and year when there is no TMDb ID.
- Keeps multi-part films apart (Part1 and Part2 are different films).
- Scores each version from Jellyfin's media info and the file name:
  resolution, source (Remux, BluRay, WEB-DL, WEBRip, HDTV, DVDRip), video codec,
  container, dubbed audio, hardcoded subtitles, and subtitle tracks.
  Hover a score to see its breakdown.
- Recommends a version to keep (✔), a version to remove (✖), or asks you to review (?).
- Groups with the same specs and size are marked as candidates. Their hash is checked only when you click **Verify identical**.
  Identical copies (same SHA-256) are marked `=`, and you choose which to keep.
- Settings: title similarity threshold, similar-title reporting, and a preference for dubbed versions.
- Each version has an **Open movie** link to its Jellyfin details page.

## Status

Early development (0.x). Deletion is not implemented, and the Delete button is a
disabled placeholder. Deletion will only be added after the detection results have been checked.

## Requirements

- Jellyfin server 12.4.0 or newer in the 12.x line
- .NET 10 SDK to build from source

## Build

```bash
dotnet build -c Release
```

The DLL is written to `src/Jellyfin.Plugin.DuplicateFinder/bin/Release/net10.0/`.

## Install (manual)

1. Stop Jellyfin.
2. Create `plugins/DuplicateFinder_<version>/` in your Jellyfin data folder,
   for example `C:\ProgramData\Jellyfin\Server\plugins\DuplicateFinder_0.3.4.0`.
3. Copy `Jellyfin.Plugin.DuplicateFinder.dll` into that folder.
4. Start Jellyfin and open **Dashboard → Plugins → Duplicate Finder**.

## Using it

Open **Settings** to adjust the options. The Detection section shows how many movies
will be checked, which items were skipped (TV episodes, and items with no year and no TMDb ID),
and a **Start detection** button. A scan only reads metadata, so it finishes in about a second.
Results appear as one table per movie.

A hash check reads both files in full. It runs only when you click **Verify identical**
on a group, one file at a time, so it does not slow down the scan.

## Versioning

This project follows [Semantic Versioning](https://semver.org/). The version in
`Jellyfin.Plugin.DuplicateFinder.csproj` and `build.yaml` must match, and each release is
tagged `vX.Y.Z`. See [CHANGELOG.md](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE).
