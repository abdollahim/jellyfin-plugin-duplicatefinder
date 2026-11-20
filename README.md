# Duplicate Finder for Jellyfin

A Jellyfin plugin that finds duplicate and similar movies in your library.
**Read-only:** it reports what it finds and never deletes or changes files.

## Install

### Option 1: Add the plugin repository (recommended)

1. In Jellyfin, go to **Dashboard → Plugins → Repositories → Add**.
2. Enter a name, such as `Duplicate Finder`, and this URL:
   ```
   https://raw.githubusercontent.com/OWNER/REPO/main/manifest.json
   ```
3. Save. Then open **Catalog**, find **Duplicate Finder**, and click **Install**.
4. Restart Jellyfin when asked.

### Option 2: Manual install

1. Download the latest zip from the [Releases page](https://github.com/OWNER/REPO/releases).
2. Stop Jellyfin.
3. Extract the DLL into a folder in your Jellyfin plugins directory, for example:
   `C:\ProgramData\Jellyfin\Server\plugins\DuplicateFinder_0.3.4.0\`
4. Start Jellyfin, then open **Dashboard → Plugins → Duplicate Finder**.

**Download link (latest release):**
https://github.com/OWNER/REPO/releases/download/v0.3.4/Jellyfin.Plugin.DuplicateFinder_0.3.4.0.zip

## Requirements

- Jellyfin server 12.4.0 or newer in the 12.x line

## Using it

Open **Settings** in the plugin page. The Detection section shows how many movies
will be checked and which items were skipped. Click **Start detection** to scan.
A scan reads metadata only and finishes in about a second.

Results appear as one table per movie. Each version shows:
- **✔ keep**, **✖ remove candidate**, or **? review**, with a keep score (higher means better to keep)
- **=** for identical copies, where you choose which one to keep
- **Open movie**, which opens the movie in Jellyfin

Groups with the same specs and size show **Same specs and size: hash not checked**.
Click **Check hash** on such a group to compare the files byte for byte.
This reads both files in full, so it can take a minute on a large file.

Settings:
- **Title similarity threshold**: how close two titles must be for "similar" matches.
- **Report similar titles**: include similar titles, not just exact duplicates.
- **Prefer dubbed versions**: when on, the non-dubbed copy becomes a removal candidate.

## Status

Early development. **Nothing is deleted.** The Delete button is a disabled placeholder.
Deletion will only be added after the detection results have been checked.

## Build from source

```bash
dotnet build -c Release
```

Requires the .NET 10 SDK. The DLL is written to
`src/Jellyfin.Plugin.DuplicateFinder/bin/Release/net10.0/`.

## Versioning

This project follows [Semantic Versioning](https://semver.org/). Each release is tagged `vX.Y.Z`.
See [CHANGELOG.md](CHANGELOG.md) for what changed in each version.

## License

MIT. See [LICENSE](LICENSE).