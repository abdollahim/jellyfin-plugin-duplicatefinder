# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and the project uses [Semantic Versioning](https://semver.org/).

## [Unreleased]

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
