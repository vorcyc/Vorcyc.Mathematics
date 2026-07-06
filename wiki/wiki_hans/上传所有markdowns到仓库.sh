#!/bin/bash
# Sync wiki_en/ and wiki_hans/ markdown into GitHub Wiki:
#   https://github.com/vorcyc/Vorcyc.Mathematics.wiki.git

set -euo pipefail

USERNAME="vorcyc"
REPOSITORY="Vorcyc.Mathematics"
WIKI_REPO="https://github.com/${USERNAME}/${REPOSITORY}.wiki.git"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
WIKI_EN="${REPO_ROOT}/wiki_en"
WIKI_HANS="${REPO_ROOT}/wiki_hans"
WORK_DIR="$(mktemp -d)"
WIKI_CLONE="${WORK_DIR}/${REPOSITORY}.wiki"

cleanup() { rm -rf "${WORK_DIR}"; }
trap cleanup EXIT

echo "Cloning ${WIKI_REPO} ..."
git clone --depth 1 "${WIKI_REPO}" "${WIKI_CLONE}"

echo "Copying English wiki (wiki_en/*.md) ..."
cp "${WIKI_EN}"/*.md "${WIKI_CLONE}/"

echo "Copying Chinese wiki (wiki_hans/*_zh.md) ..."
cp "${WIKI_HANS}"/*_zh.md "${WIKI_CLONE}/"

cd "${WIKI_CLONE}"
git add -A

if git diff --staged --quiet; then
  echo "No wiki changes to push."
  exit 0
fi

MSG="Sync wiki from Vorcyc.Mathematics ($(date -u +%Y-%m-%dT%H:%MZ))"
git commit -m "${MSG}"
git push origin HEAD

echo "Wiki pushed: ${WIKI_REPO}"
