# Releasing

Same process and CI jobs as the Elympics SDK (see its `.claude/RELEASING.md` and the SDK wiki page "Contributing and releasing"). `develop` and `main` are protected, so every merge goes through an MR.

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
