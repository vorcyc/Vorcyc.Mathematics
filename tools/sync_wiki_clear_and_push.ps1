# Clear GitHub wiki repo and upload latest wiki_en + wiki_hans docs.
$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$WikiRepo = 'https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git'
$WorkDir = Join-Path $env:TEMP "vmath-wiki-$(Get-Random)"
$CloneDir = Join-Path $WorkDir 'Vorcyc.Mathematics.wiki'

try {
    New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
    Write-Host "Cloning $WikiRepo ..."
    & git clone $WikiRepo $CloneDir
    if ($LASTEXITCODE -ne 0) { throw "git clone failed ($LASTEXITCODE)" }

    $before = (Get-ChildItem -Path $CloneDir -Filter '*.md' -File -ErrorAction SilentlyContinue).Count
    Write-Host "Existing wiki pages: $before"

    Write-Host 'Removing all existing wiki files ...'
    Get-ChildItem -Path $CloneDir -Force | Where-Object { $_.Name -ne '.git' } | Remove-Item -Recurse -Force

    Write-Host 'Copying wiki_en/*.md and wiki_hans/*_zh.md ...'
    Copy-Item (Join-Path $RepoRoot 'wiki_en\*.md') $CloneDir -Force
    Copy-Item (Join-Path $RepoRoot 'wiki_hans\*_zh.md') $CloneDir -Force

    Push-Location $CloneDir
    $enCount = (Get-ChildItem -Filter '*.md' | Where-Object { $_.Name -notmatch '_zh\.md$' }).Count
    $zhCount = (Get-ChildItem -Filter '*_zh.md').Count
    Write-Host "New wiki pages: $enCount EN + $zhCount ZH = $($enCount + $zhCount)"

    & git add -A
    $staged = & git diff --staged --name-only
    if ([string]::IsNullOrWhiteSpace($staged)) {
        Write-Host 'No changes to push.'
        return
    }
    Write-Host "Staged change count: $($staged.Count)"

    $msg = "Clear wiki and sync latest EN/ZH docs from Vorcyc.Mathematics ($(Get-Date -Format 'yyyy-MM-ddTHH:mmZ'))"
    & git -c user.name='cyclone_dll' -c user.email='vorcyc@users.noreply.github.com' commit -m $msg
    if ($LASTEXITCODE -ne 0) { throw "git commit failed ($LASTEXITCODE)" }

    & git push origin HEAD
    if ($LASTEXITCODE -ne 0) { throw "git push failed ($LASTEXITCODE)" }

    Write-Host "SUCCESS: $WikiRepo"
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
}
