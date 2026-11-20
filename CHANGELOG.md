# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.3.4] - 2026-11-20

### Added
- "Verify identical (hash)" button on groups with the same specs and size. The hash is
  checked only when you click it, and it runs in the background.

### Changed
- The scan reads metadata only, so it finishes in about a second again. Hashing no longer
  runs during the scan.
- Groups with the same specs and size are shown as "not verified" until they are checked.
- Open movie and Delete (later) use the same button shape. Only the colors differ.
- Page styling uses inline styles only, so pills and buttons appear as designed.

### Fixed
- Scans that ran for several minutes because every matching group was hashed during the scan.

## [0.3.3] - 2026-11-20

### Added
- Identical copies: groups whose specs and SHA-256 hash match are marked "Identical".
  Their rows show `=`, with no keep or remove recommendation. You choose which copy to keep.
- Same specs with different content are marked "same specs, different content" and sent for review.
- Hashing runs only for groups whose specs and size already match, to keep scans fast.
- Colored pills for group type and match confidence, and colored action buttons.
- README rewritten to describe the current features.

### Changed
- Results use more of the page width, with matching left and right padding.

## [0.3.2] - 2026-11-20

### Added
- "Open movie" button under the disabled Delete placeholder. It opens the movie's
  details page in the Jellyfin web UI in a new tab.
- Each version now carries its Jellyfin item ID.

### Fixed
- The last column touched the scrollbar edge. The table area now has right padding.

## [0.3.1] - 2026-11-20

### Changed
- Score column renamed to "Keep score" (higher means better to keep).
- Values are vertically centered. File names are left aligned and wrap.
- Tables use the full page width.

### Fixed
- Cell alignment attributes were duplicated, so centering did not apply.

## [0.3.0] - 2026-11-20

### Added
- Results shown as a table per movie: title as the heading, then one row per version
  with keep / remove / review, file, size, resolution, codec, source, dubbed,
  subtitles, score, and a disabled "Delete (later)" placeholder.
- Version scoring from Jellyfin's media info and file names: resolution, source,
  codec, container, dubbed audio, hardcoded subtitles, and subtitle tracks.
  Every point is listed in the score tooltip.
- "Prefer dubbed versions" setting. When on, the non-dubbed copy is a removal candidate.
- Groups are labeled "different versions" or "same size (possible copy)".
- Title-and-year matching for movies without a TMDb ID, marked as low confidence.

### Changed
- Multi-part films are grouped by part number, so Part1 and Part2 are no longer merged.
- Read-only: nothing is deleted or changed on disk.

## [0.2.2] - 2026-11-20

### Fixed
- Settings page did not show statistics or status. The page read camelCase
  property names, but Jellyfin's API returns PascalCase. Errors now appear on the page.

## [0.2.1] - 2026-11-20

### Fixed
- Build error: `BaseItemKind` namespace. Version 0.2.0 did not compile.

## [0.2.0] - 2026-11-20

### Added
- Statistics on the settings page: total movies, checked by TMDb ID, checked by
  title and year, multi-part films, and the number of movies that will be checked.
- Skipped-item report: TV episodes and items with no year and no TMDb ID.
- Exact duplicate detection by TMDb ID, run in the background, with a status line
  and results showing file sizes.
- API endpoints under `DuplicateFinder/` (`Stats`, `Start`, `Status`).

### Fixed
- Build errors caused by missing `using` directives in the new model and service files.

## [0.1.0] - 2026-11-20

### Added
- Initial plugin skeleton based on the Jellyfin plugin template.
- Settings page with a title similarity threshold and a similar-titles option.
- Plugin GUID and build metadata for Jellyfin 12.4.0.
