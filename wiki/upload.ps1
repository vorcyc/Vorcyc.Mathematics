# Sync wiki/ to GitHub Wiki: https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git

$ErrorActionPreference = 'Stop'
$WikiRoot = $PSScriptRoot
$WikiRepo = 'https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git'
$WorkDir = Join-Path $env:TEMP "vmath-wiki-$(Get-Random)"
$CloneDir = Join-Path $WorkDir 'Vorcyc.Mathematics.wiki'

function Ensure-Clone {
    New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
    Write-Host "Cloning $WikiRepo ..."
    git clone $WikiRepo $CloneDir 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'Clone failed; trying empty wiki init ...'
        New-Item -ItemType Directory -Path $CloneDir -Force | Out-Null
        Push-Location $CloneDir
        git init | Out-Null
        git remote add origin $WikiRepo
        git checkout -b master 2>$null
        if ($LASTEXITCODE -ne 0) { git checkout -b main 2>$null }
        Pop-Location
    }
}

function Copy-WikiFiles {
    $en = Join-Path $WikiRoot 'wiki_en\*.md'
    $zh = Join-Path $WikiRoot 'wiki_hans\*_zh.md'
    $sidebar = Join-Path $WikiRoot '_Sidebar.md'

    if (-not (Test-Path (Join-Path $WikiRoot 'wiki_en\HOME.md'))) {
        throw "Missing wiki_en\HOME.md — run this script from the wiki folder."
    }

    # Drop legacy double-extension pages if present from an earlier upload style.
    Get-ChildItem $CloneDir -File -Filter '*.md.md' -ErrorAction SilentlyContinue | Remove-Item -Force

    Write-Host 'Copying wiki_en/*.md, wiki_hans/*_zh.md, _Sidebar.md ...'
    Copy-Item $en $CloneDir -Force
    Copy-Item $zh $CloneDir -Force
    Copy-Item $sidebar $CloneDir -Force

    $count = (Get-ChildItem $CloneDir -Filter '*.md').Count
    Write-Host "Copied $count markdown file(s)."
    if ($count -lt 10) {
        throw "Too few files copied ($count). Check paths under $WikiRoot"
    }
}

try {
    Ensure-Clone
    Copy-WikiFiles

    Push-Location $CloneDir
    git add -A
    $staged = git diff --staged --name-only
    if ([string]::IsNullOrWhiteSpace($staged)) {
        Write-Host 'Nothing to commit (working tree unchanged).'
        return
    }

    Write-Host 'Staged files:'
    $staged | ForEach-Object { Write-Host "  $_" }

    $msg = "Sync wiki from Vorcyc.Mathematics ($(Get-Date -Format 'yyyy-MM-ddTHH:mmZ'))"
    git -c user.name='cyclone_dll' -c user.email='vorcyc@users.noreply.github.com' commit -m $msg
    if ($LASTEXITCODE -ne 0) { throw "git commit failed ($LASTEXITCODE)" }

    $branch = (git branch --show-current)
    if ([string]::IsNullOrWhiteSpace($branch)) { $branch = 'master' }
    git push -u origin $branch
    if ($LASTEXITCODE -ne 0) {
        git push -u origin HEAD:master
        if ($LASTEXITCODE -ne 0) { throw "git push failed ($LASTEXITCODE)" }
    }

    Write-Host "Wiki pushed: $WikiRepo"
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
}
