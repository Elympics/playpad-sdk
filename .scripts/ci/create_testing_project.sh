#!/usr/bin/env bash
# Creates the testing project in UNITY_DIR with the package embedded in PACKAGE_DIR and the Elympics SDK from git.
# ProjectSettings/ and Library/ come from the runner cache; without them, two Unity runs create the project and import it.
set -e

run_unity() {
  "$(dirname "${BASH_SOURCE[0]}")/unity_editor.sh" \
    "$@" \
    -logFile /dev/stdout \
    -batchmode \
    -nographics \
    -quit
}

echo "Creating Testing Project"
PACKAGE_FOLDER=${PACKAGE_DIR}/

NEW_PROJECT=
if [ -f "${UNITY_DIR}/ProjectSettings/ProjectSettings.asset" ]; then
  echo "ProjectSettings restored from the cache"
else
  # -createProject adds fewer default packages than opening a folder without ProjectSettings/.
  echo "Creating ProjectSettings"
  rm -rf "$UNITY_DIR"
  run_unity -createProject "$UNITY_DIR" || { UNITY_EXIT_CODE=$?; echo "Failed to create project"; exit "$UNITY_EXIT_CODE"; }
  NEW_PROJECT=1
fi

echo "Moving Package files to appropriate directories..."

mkdir -p "$PACKAGE_FOLDER" "${UNITY_DIR}/Assets"
cp -r Editor{,.meta} "$PACKAGE_FOLDER"
cp -r Runtime{,.meta} "$PACKAGE_FOLDER"
cp -r Tests{,.meta} "$PACKAGE_FOLDER"
cp package.json{,.meta} "$PACKAGE_FOLDER"
cp -r WebGLTemplates~ "$PACKAGE_FOLDER"
cp -r Samples~ "$PACKAGE_FOLDER"
mv "${PACKAGE_FOLDER}Samples~" "${PACKAGE_FOLDER}Samples"

echo "Elympics Playpad package moved ✅"

echo "Adding Elympics SDK dependency..."

echo "Using Elympics SDK git path: $CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE"
if git ls-remote --exit-code --heads -- "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" "refs/heads/$CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE"
then
  :
elif git ls-remote --exit-code --heads -- "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" "refs/heads/develop"
then
  CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE="develop"
else
  CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE="$(git ls-remote -- "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" HEAD | sed 's/\s\+HEAD$//i')"
fi
echo "Using Elympics SDK commit ref: $CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE"
mkdir -p "$(dirname "$PACKAGES_MANIFEST_PATH")"
# Rider: generate_solution_files.sh calls its SyncSolution
SDK_URL="$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE#$CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE" \
  yq -n -o json '.dependencies[strenv(ELYMPICS_SDK_PACKAGE_NAME)] = strenv(SDK_URL) | .dependencies."com.unity.ide.rider" = "3.0.39"' \
  > "$PACKAGES_MANIFEST_PATH"

echo "Elympics SDK dependency added ✅"

if [ -n "$NEW_PROJECT" ]; then
  echo "Importing the project"
  run_unity -projectPath "$UNITY_DIR" || { UNITY_EXIT_CODE=$?; echo "Failed to import project"; exit "$UNITY_EXIT_CODE"; }
fi

# Keeps com.unity.webrtc's native context stopped in the Editor until a test needs it; it can crash Unity during reloads.
"$(dirname "${BASH_SOURCE[0]}")/set_scripting_defines.sh" "ELYMPICS_LAZY_WEBRTC_CONTEXT"
