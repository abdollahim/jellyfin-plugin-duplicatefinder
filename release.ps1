<#
.SYNOPSIS
  Builds, packages, and (optionally) publishes a Duplicate Finder release.

.DESCRIPTION
  Steps:
    1. Checks the version in the .csproj and build.yaml, and that placeholders are filled in.
    2. Checks that the git working tree is clean (tracked files only).
    3. Creates the tag vX.Y.Z on HEAD if it does not exist. Stops if it exists on another commit.
    4. Builds in Release mode.
    5. Writes the plugin zip, its SHA-256 file, and release-notes.md from CHANGELOG.md.
    6. Pushes the branch and tag (only if the tag was created by this run).
    7. With -CreateRelease, creates the GitHub release using the GitHub CLI (gh).

  Nothing is committed for you. Commit your changes first.

.EXAMPLE
  .\release.ps1 -Version 0.3.4
  .\release.ps1 -Version 0.3.4 -CreateRelease
#>
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [switch]$CreateRelease
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Fail($message) { throw "Release stopped: $message" }
function Run($cmd, [string[]]$arguments) {
    & $cmd @arguments
    if ($LASTEXITCODE -ne 0) { Fail "'$cmd $($arguments -join ' ')' failed with exit code $LASTEXITCODE" }
}

$tag = "v$Version"
$fourPart = "$Version.0"
$project = "src\Jellyfin.Plugin.DuplicateFinder"
$csproj = Join-Path $project "Jellyfin.Plugin.DuplicateFinder.csproj"
$dllPath = Join-Path $project "bin\Release\net10.0\Jellyfin.Plugin.DuplicateFinder.dll"
$outDir = Join-Path $PSScriptRoot "release"
$zipName = "Jellyfin.Plugin.DuplicateFinder_$fourPart.zip"
$zipPath = Join-Path $outDir $zipName
$shaPath = "$zipPath.sha256"
$notesPath = Join-Path $outDir "release-notes.md"

Write-Host "== 1. Checking version and placeholders" -ForegroundColor Cyan
$csprojVersion = [regex]::Match((Get-Content $csproj -Raw), '<Version>([^<]+)</Version>').Groups[1].Value
if ($csprojVersion -ne $fourPart) { Fail "csproj Version is '$csprojVersion', expected '$fourPart'." }

$buildYaml = Get-Content build.yaml -Raw
if ($buildYaml -notmatch "version:\s*`"$fourPart`"") { Fail "build.yaml version does not match '$fourPart'." }

$placeholders = Select-String -Path build.yaml, LICENSE -Pattern 'YOUR_' -ErrorAction SilentlyContinue
if ($placeholders) { Fail "Replace the YOUR_ placeholders first:`n$($placeholders | Out-String)" }

if (-not (Select-String -Path CHANGELOG.md -Pattern ([regex]::Escape("## [$Version]")) -Quiet)) {
    Fail "CHANGELOG.md has no section for [$Version]."
}

Write-Host "== 2. Checking git working tree" -ForegroundColor Cyan
$dirty = git status --porcelain --untracked-files=no
if ($LASTEXITCODE -ne 0) { Fail "Not a git repository, or git is not installed." }
if ($dirty) { Fail "Uncommitted changes. Commit them first:`n$dirty" }

Write-Host "== 3. Tag $tag" -ForegroundColor Cyan
$head = git rev-parse HEAD
$tagExists = git tag --list $tag
$createdTag = $false
if ($tagExists) {
    $tagCommit = git rev-list -n 1 $tag
    if ($tagCommit -ne $head) {
        Fail "Tag $tag already points to $tagCommit, not HEAD ($head). Do not move published tags. Use a new version."
    }
    Write-Host "Tag $tag already exists on HEAD. Using it."
} else {
    git tag -a $tag -m "Release $Version" HEAD
    if ($LASTEXITCODE -ne 0) { Fail "Could not create tag $tag." }
    $createdTag = $true
    Write-Host "Created tag $tag."
}

Write-Host "== 4. Building Release" -ForegroundColor Cyan
Run "dotnet" @("build", "-c", "Release")
if (-not (Test-Path $dllPath)) { Fail "DLL not found at $dllPath." }

Write-Host "== 5. Packaging" -ForegroundColor Cyan
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
Remove-Item $zipPath, $shaPath -ErrorAction SilentlyContinue
Compress-Archive -Path $dllPath -DestinationPath $zipPath -Force

$hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
"$hash  $zipName" | Set-Content -Path $shaPath -Encoding ascii

$changelog = Get-Content CHANGELOG.md -Raw
$section = [regex]::Match($changelog, "(?s)## \[$([regex]::Escape($Version))\].*?(?=\r?\n## \[|\z)").Value.Trim()
$section | Set-Content -Path $notesPath -Encoding utf8

Write-Host "Zip:     $zipPath"
Write-Host "SHA-256: $hash"
Write-Host "Notes:   $notesPath"

Write-Host "== 6. Pushing" -ForegroundColor Cyan
Run "git" @("push")
if ($createdTag) {
    Run "git" @("push", "origin", $tag)
} else {
    Write-Host "Tag was already on the remote or local repo. Not pushed again."
}

if ($CreateRelease) {
    Write-Host "== 7. GitHub release" -ForegroundColor Cyan
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        Fail "GitHub CLI (gh) is not installed. Install it from https://cli.github.com, or publish the zip manually."
    }
    gh release view $tag *> $null
    if ($LASTEXITCODE -eq 0) {
        Fail "Release $tag already exists on GitHub. Edit it there, or use a new version."
    }
    Run "gh" @("release", "create", $tag, $zipPath, $shaPath, "--title", "Duplicate Finder $Version", "--notes-file", $notesPath)
    Write-Host "Release $tag published."
} else {
    Write-Host ""
    Write-Host "Done. To publish, create a release for $tag on GitHub and attach:" -ForegroundColor Green
    Write-Host "  $zipPath"
    Write-Host "  $shaPath"
    Write-Host "Or run again with -CreateRelease (needs GitHub CLI)."
}