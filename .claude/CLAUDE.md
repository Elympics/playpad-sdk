# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`com.daftmobile.elympics-playpad` is the Elympics PlayPad SDK, a UPM package that depends on the Elympics SDK (`com.daftmobile.elympics`, version pinned in `package.json`). It's developed on GitLab (`elympics/sdk/playpad-sdk`) and mirrored to GitHub as `Elympics/playpad-sdk`.

- CI tests the package against the Elympics SDK repository's default branch (`CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE`), not against the pinned version.
- `Runtime/Plugins/MessagePackGenerated.cs` comes from `.scripts/generate_message_pack.sh`. Re-run it whenever a lobby or Rooms `[MessagePackObject]` type changes, because CI (`verify-messagepack-generated`) regenerates the file and fails on any difference.

## Releasing

See `.claude/RELEASING.md`.
