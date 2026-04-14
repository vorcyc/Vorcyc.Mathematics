# Sync wiki_en/ and wiki_hans/ to GitHub Wiki:
# https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git

$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$WikiRepo = 'https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git'
$WorkDir = Join-Path $env:TEMP "vmath-wiki-$(Get-Random)"
$CloneDir = Join-Path $WorkDir 'Vorcyc.Mathematics.wiki'

try {
    New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
    Write-Host "Cloning $WikiRepo ..."
    git clone --depth 1 $WikiRepo $CloneDir
    if ($LASTEXITCODE -ne 0) { throw "git clone failed ($LASTEXITCODE)" }

    Write-Host 'Copying wiki_en/*.md and wiki_hans/*_zh.md ...'
    Copy-Item (Join-Path $RepoRoot 'wiki_en\*.md') $CloneDir -Force
    Copy-Item (Join-Path $RepoRoot 'wiki_hans\*_zh.md') $CloneDir -Force

    Push-Location $CloneDir
    git add -A
    if ([string]::IsNullOrWhiteSpace((git diff --staged --name-only))) {
        Write-Host 'No wiki changes to push.'
        return
    }

    $msg = "Sync wiki from Vorcyc.Mathematics ($(Get-Date -Format 'yyyy-MM-ddTHH:mmZ'))"
    git -c user.name='cyclone_dll' -c user.email='vorcyc@users.noreply.github.com' commit -m $msg
    if ($LASTEXITCODE -ne 0) { throw "git commit failed ($LASTEXITCODE)" }

    git push origin HEAD
    if ($LASTEXITCODE -ne 0) { throw "git push failed ($LASTEXITCODE)" }

    Write-Host "Wiki pushed: $WikiRepo"
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
}
