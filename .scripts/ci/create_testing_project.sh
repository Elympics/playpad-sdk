#!/usr/bin/env bash
set -e

echo "Creating Testing Project"
PACKAGE_FOLDER=${PACKAGE_DIR}/

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -createProject "$UNITY_DIR" \
  -logFile /dev/stdout \
  -batchmode \
  -nographics \
  -quit

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred"
else
  echo "Failed to create project"
  exit 1
fi

echo "Moving Package files to appropriate directories..."

mkdir -p "$PACKAGE_FOLDER"
cp -r Editor{,.meta} "$PACKAGE_FOLDER"
cp -r Runtime{,.meta} "$PACKAGE_FOLDER"
cp -r Tests{,.meta} "$PACKAGE_FOLDER"
cp package.json{,.meta} "$PACKAGE_FOLDER"
cp -r Samples~ "$PACKAGE_FOLDER"
mv "${PACKAGE_FOLDER}Samples~" "${PACKAGE_FOLDER}Samples"

echo "Elympics Playpad package moved ✅"

echo "Adding Elympics SDK dependency..."

CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE="${CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE:-https://github.com/Elympics/Unity-SDK.git}"
echo "Using Elympics SDK git path: $CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE"
DEFAULT_HASH="$(git ls-remote "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" HEAD | sed 's/\s\+HEAD$//i')"
yq -iPo json '.dependencies."'"$ELYMPICS_SDK_PACKAGE_NAME"'" = "'"$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE"'"' "$PACKAGES_MANIFEST_PATH"
yq -iPo json '.dependencies."'"$ELYMPICS_SDK_PACKAGE_NAME"'" = { "version": "'"$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE"'", "source": "git" }' "$PACKAGES_LOCK_PATH"

if git ls-remote --exit-code --heads "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" "refs/heads/$CI_COMMIT_REF_NAME"
then
  CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE="${CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE:-$CI_COMMIT_REF_NAME}"
elif git ls-remote --exit-code --heads "$CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE" "refs/heads/develop"
then
  CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE="${CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE:-develop}"
else
  CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE="${CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE:-$DEFAULT_HASH}"
fi
echo "Using Elympics SDK commit ref: $CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE"
yq -iPo json '.dependencies."'"$ELYMPICS_SDK_PACKAGE_NAME"'".hash = "'"$CI_ELYMPICS_SDK_COMMIT_REF_OVERRIDE"'"' "$PACKAGES_LOCK_PATH"

echo "Elympics SDK dependency added ✅"
