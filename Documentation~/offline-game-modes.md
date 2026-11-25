# Offline game modes

The host (for example JoyFlick) can launch your game in a mode that has no match, such as an onboarding run for a new guest. PlayPad passes that mode to the game and stops the session before authentication, so the game can run its offline content.

## `gameMode` values

The host sends `gameMode` in the handshake response.

| Value | Meaning |
|---|---|
| missing, empty, blank or `standard` | The standard online flow. Nothing changes. |
| anything else, for example `onboarding` or `onboarding_ghost` | An offline mode. The value is trimmed and compared case-sensitively. |

A game that doesn't know an offline value should start nothing and log an error.

## What the session does in an offline mode

`SessionManager.AuthenticateFromExternalAndConnect()` runs the handshake, then returns normally:

- It does not authenticate, look up the region, start matchmaking or the lobby, or watch for authentication changes.
- `ISessionManager.IsOffline` is `true` and `ISessionManager.GameMode` holds the mode.
- `CurrentSession` stays `null`, so don't read it without checking `IsOffline`.
- `FinishSessionInfoUpdate` is raised once, and a second call to `AuthenticateFromExternalAndConnect()` still throws "already initialized".

What keeps working: the heartbeat (it starts with the handshake), `HideSplashScreen`, breadcrumbs and `Quit`.

```csharp
await sessionManager.AuthenticateFromExternalAndConnect();
if (sessionManager.IsOffline)
{
    RunOfflineContent(sessionManager.GameMode!);
    return;
}
```

## Your game's part

The game runs the offline content itself, in one process, and ends it by sending `Quit` with the score (`QuitArgs { Score = ... }`). The host decides what happens next.

Guests get an empty `jwt` from the host. An offline mode never asks for authentication, so this doesn't fail. In the standard flow an empty `jwt` still fails as before.

## Developer override

In the Editor and non-WebGL players, the `ELYMPICS_GAME_MODE` launch parameter replaces the handshake's game mode. Use it to try an offline mode without a host:

| Source | Name |
|---|---|
| Environment variable | `ELYMPICS_GAME_MODE` |
| Command-line argument | `--elympics-game-mode=onboarding` |

A non-empty value wins and goes through the same normalisation, so `standard` forces the online flow. The override is ignored in WebGL players.

## Testing in a WebGL build

The bundled `ElympicsLobby` WebGL template (v0.7.0 and later) sends the config's `gameMode` in local mode. Open the build with `?gameMode=onboarding` (or `?gameMode=onboarding_ghost`) to start the offline flow. Without the parameter, or with `?gameMode=standard`, the standard flow runs.
