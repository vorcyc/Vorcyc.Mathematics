#!/bin/bash
# Sync wiki/ to GitHub Wiki: https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git
# Windows: use Git Bash, or run upload.ps1 in PowerShell instead.

set -euo pipefail

WIKI_REPO="https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git"
WIKI_ROOT="$(cd "$(dirname "$0")" && pwd)"
WORK_DIR="$(mktemp -d)"
WIKI_CLONE="${WORK_DIR}/Vorcyc.Mathematics.wiki"

cleanup() { rm -rf "${WORK_DIR}"; }
trap cleanup EXIT

if [[ ! -f "${WIKI_ROOT}/wiki_en/HOME.md" ]]; then
  echo "Missing wiki_en/HOME.md — run from the wiki/ folder." >&2
  exit 1
fi

echo "Cloning ${WIKI_REPO} ..."
if ! git clone "${WIKI_REPO}" "${WIKI_CLONE}" 2>/dev/null; then
  echo "Clone failed; initializing empty wiki repo ..."
  mkdir -p "${WIKI_CLONE}"
  git -C "${WIKI_CLONE}" init
  git -C "${WIKI_CLONE}" remote add origin "${WIKI_REPO}"
  git -C "${WIKI_CLONE}" checkout -b master 2>/dev/null || git -C "${WIKI_CLONE}" checkout -b main
fi

echo "Copying markdown files ..."
rm -f "${WIKI_CLONE}"/*.md.md 2>/dev/null || true
cp "${WIKI_ROOT}/wiki_en"/*.md "${WIKI_CLONE}/"
cp "${WIKI_ROOT}/wiki_hans"/*_zh.md "${WIKI_CLONE}/"
cp "${WIKI_ROOT}/_Sidebar.md" "${WIKI_CLONE}/"

COUNT="$(find "${WIKI_CLONE}" -maxdepth 1 -name '*.md' | wc -l | tr -d ' ')"
echo "Copied ${COUNT} markdown file(s)."
if [[ "${COUNT}" -lt 10 ]]; then
  echo "Too few files copied — check paths under ${WIKI_ROOT}" >&2
  exit 1
fi

cd "${WIKI_CLONE}"
git add -A

if git diff --staged --quiet; then
  echo "Nothing to commit."
  exit 0
fi

MSG="Sync wiki from Vorcyc.Mathematics ($(date -u +%Y-%m-%dT%H:%MZ))"
git -c user.name='cyclone_dll' -c user.email='vorcyc@users.noreply.github.com' commit -m "${MSG}"

BRANCH="$(git branch --show-current || true)"
if [[ -z "${BRANCH}" ]]; then BRANCH="master"; fi
git push -u origin "${BRANCH}" || git push -u origin HEAD:master

echo "Wiki pushed: ${WIKI_REPO}"
