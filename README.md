# Duplicate Finder for Jellyfin

A Jellyfin plugin that finds duplicate and similar movies in your library.
It is **read-only**: it reports groups and never deletes files.

## Status

Early development (0.x). Exact duplicates by TMDb ID work. Title-and-year
and similar-title matching are not ported yet. Deletion is not planned until
accuracy is verified.

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
2. Create `plugins/DuplicateFinder_<version>/` in your Jellyfin data folder
   (for example `C:\ProgramData\Jellyfin\Server\plugins\DuplicateFinder_0.2.0.0`).
3. Copy `Jellyfin.Plugin.DuplicateFinder.dll` into that folder.
4. Start Jellyfin and open **Dashboard → Plugins → Duplicate Finder**.

## Using it

Open **Settings** to change the title similarity threshold and whether similar
titles are reported. The Detection section shows how many movies will be checked,
which items are skipped (TV episodes, items with no year and no TMDb ID), and
a **Start detection** button.

## Versioning

This project follows [Semantic Versioning](https://semver.org/). The version in
`Jellyfin.Plugin.DuplicateFinder.csproj` and `build.yaml` must match, and each
release is tagged `vX.Y.Z`. See [CHANGELOG.md](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE).
