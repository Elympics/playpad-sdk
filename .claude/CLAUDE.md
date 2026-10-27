# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`com.daftmobile.elympics-playpad` is the Elympics PlayPad SDK, a UPM package that depends on the Elympics SDK (`com.daftmobile.elympics`, version pinned in `package.json`). It's developed on GitLab (`elympics/sdk/playpad-sdk`) and mirrored to GitHub as `Elympics/playpad-sdk`.

- CI tests the package against the Elympics SDK repository's default branch (`CI_ELYMPICS_SDK_GIT_PATH_OVERRIDE`), not against the pinned version.
- `Runtime/Plugins/MessagePackGenerated.cs` comes from `.scripts/generate_message_pack.sh`. Re-run it whenever a lobby or Rooms `[MessagePackObject]` type changes, because CI (`verify-messagepack-generated`) regenerates the file and fails on any difference.

## Editor CLI commands

`Editor/UnityCli/` is the `ElympicsPlayPad.Editor.UnityCli` assembly: `[CliCommand]`s for `com.unity.pipeline` (a package dependency), built on the SDK's `Elympics.Editor.UnityCli` helpers (`CliRun`, `CliArgs`, `CliWeb`).

- Commands are `playpad_<verb>_<noun>`, and their methods are named verb-first too:
  - `playpad_get_template_status`, `playpad_install_template`, `playpad_get_template_values`, `playpad_set_template_values` (WebGL template, `PlayPadTemplateCliCommands`).
  - `playpad_upload_client_build`, `playpad_upload_streaming_assets` (`PlayPadUploadCliCommands`).
- The Manage Games window sections that do the same uploads are in `Editor/Build/`. The template's `TEMPLATE_*` environment variables and `ELYMPICS_*` custom values are defined once, in `PlayPadPreProcessBuild.TemplateValues`.
- Tests (`Tests/Editor/`, category `UnityCli`) check the error code and the "Fix:" hint with `PlayPadCliAssert.HasCodeAndFix`.

## Releasing

See `.claude/RELEASING.md`.
