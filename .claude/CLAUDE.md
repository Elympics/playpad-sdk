# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`com.daftmobile.elympics-playpad` is the Elympics PlayPad SDK, a UPM package that depends on the Elympics SDK (`com.daftmobile.elympics`, version pinned in `package.json`). It's developed on GitLab (`elympics/sdk/playpad-sdk`) and mirrored to GitHub as `Elympics/playpad-sdk`.

- CI tests the package against the Elympics SDK repository's default branch (`CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE`), not against the pinned version.
- `Runtime/Plugins/MessagePackGenerated.cs` comes from `.scripts/generate_message_pack.sh`. Re-run it whenever a lobby or Rooms `[MessagePackObject]` type changes, because CI (`verify-messagepack-generated`) regenerates the file and fails on any difference.

## Releasing

Same process and CI jobs as the Elympics SDK (see its `.claude/CLAUDE.md`, "Releasing", and the SDK wiki page "Contributing and releasing"). `develop` and `main` are protected, so every merge goes through an MR.

1. **Release the Elympics SDK first** if PlayPad needs its new API. Then, through an MR to `develop`, set `"com.daftmobile.elympics"` in `package.json` to the released SDK version. CI doesn't bump this dependency.
2. **Cut the branch:** `release/vX.Y.Z` from `develop` (`hotfix/vX.Y.Z` from `main`). PlayPad is past 1.0, so follow semver strictly: removing or changing public API needs a major version.
3. **Let CI bump:** `create-changelog` commits the version (`package.json`, `Runtime/AssemblyInfo.cs`) and the `CHANGELOG.md` entry as Elympics Bot. Pull it. Never bump or tag by hand.
4. **Fix the changelog:** commit edits as `chore: update changelog`. If nothing needs editing, push an empty `trigger pipeline` commit instead. The generator has the same quirks as in the SDK:
   - `type!:` commits land in a group without a heading.
   - BREAKING notes keep `Co-Authored-By` trailers and footers that later commits superseded.
   - Removals made with commits that have no `BREAKING CHANGE` footer are missing.
5. **Merge into `main`:** open an MR from the release branch to `main` and merge it without squashing.
6. **Publish:** trigger the manual `release:gitlab` job on `main`. It creates the tag and the GitLab release. The tag pipeline's `release:github` then publishes to GitHub. If it fails because the tag hasn't been mirrored yet, retry it.
7. **Merge back:** open an MR from `main` to `develop`. Skipping this leaves the release tags off `develop`'s history, and the next changelog would then start from an older tag.

CI needs these variables: `GITLAB_ACCESS_TOKEN` (for the bot commit), `GH_TOKEN` and `GH_REPO`.
