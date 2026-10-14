#!/usr/bin/env bash
# Runs Project Auditor on the testing project and compares this package's domain-reload and upgrade findings with
# .scripts/ci/project-auditor-baseline.txt (see ProjectAuditorCi.cs). Expects UNITY_DIR and PACKAGE_DIR.
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MANIFEST="$UNITY_DIR/Packages/manifest.json"

jq '.dependencies["com.unity.project-auditor"] = "3.1.1"' "$MANIFEST" > "$MANIFEST.tmp" && mv "$MANIFEST.tmp" "$MANIFEST"
# Project Auditor recompiles every assembly against all precompiled DLLs. The tests' NSubstitute.dll brings its own
# ValueTask, so MessagePack, UniTask and everything built on them fail that compile and are skipped without an error.
rm -rf "$PACKAGE_DIR/Tests" "$PACKAGE_DIR/Tests.meta"
# Elympics (with the DLL) comes from git into Library/PackageCache; embed the commit it resolves to, without its tests.
ELYMPICS_DIR="$UNITY_DIR/Packages/$ELYMPICS_SDK_PACKAGE_NAME"
ELYMPICS_REF=$(jq -r '.dependencies["'"$ELYMPICS_SDK_PACKAGE_NAME"'"].hash' "$PACKAGES_LOCK_PATH")
git init -q "$ELYMPICS_DIR"
git -C "$ELYMPICS_DIR" fetch -q --depth 1 "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" "$ELYMPICS_REF"
git -C "$ELYMPICS_DIR" checkout -q FETCH_HEAD
rm -rf "$ELYMPICS_DIR/.git" "$ELYMPICS_DIR/Tests" "$ELYMPICS_DIR/Tests.meta"
mkdir -p "$UNITY_DIR/Assets/Editor"
cp "$SCRIPT_DIR/ProjectAuditorCi.cs" "$UNITY_DIR/Assets/Editor/ProjectAuditorCi.cs"

export CI_AUDIT_PACKAGE_PATH="Packages/$(basename "$PACKAGE_DIR")/"
export CI_AUDIT_BASELINE="$SCRIPT_DIR/project-auditor-baseline.txt"
export CI_AUDIT_OUTPUT="$CI_PROJECT_DIR/project-auditor-findings.txt"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath "$UNITY_DIR" \
  -executeMethod ProjectAuditorCi.Run \
  -quit \
  -logFile /dev/stdout \
  -batchmode \
  -nographics
