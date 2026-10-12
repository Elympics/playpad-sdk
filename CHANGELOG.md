## 3.0.0 (2026-10-12)


### Bug Fixes 🪲

* Assign the play button view in the async sample (4905cc0)
* Correct "received" typo in the lobby status log (d9e9b0c)
* Don't replay a finished web-lobby match (775e50b)
* dontdestroyonload for root object (8ff7f05)
* Follow the web host's match flow in the async sample (506a153)
* Import LaunchedMatch outside the lobby strip gate (44fb8c6)
* Initialize the lobby wrapper before PlayPadCommunicator (751fc57)
* Keep only the coin catalog of the virtual-deposit communicator (1a6471d)
* Launch lobby matches only from an idle launcher (8d8888e)
* Make PlayPad SDK compatible with Elympics SDK update (f76421a)
* Match the installed ElympicsLobby WebGL template in the build preprocessor (1e5c129)
* Play quick matches in the async sample without the tournament feature (e4e9615)
* Raise web requests from JS as web requests (07a4e0b)
* Remove the unused ElympicsGetHref from the PlayPad jslib (4dc3c85)
* Show a working Play button in the async sample without a tournament (07c2d7b)
* Treat only a Gameplay-mode session as host-driven in the async sample (663577c)
* Use a test ElympicsConfig in tests (abdd681)


### Features

* Drive matches through ElympicsMatchLauncher (a84e086)
* Forward lost packet info in NetworkStatusMessages, add void messages for showing/hiding modals (4902597)
* Include warning-type logs in breadcrumbs (6d5dca8)
* Restore Rooms in ElympicsPlayPad.Rooms (8f1ea8d)
* Restore the auth client in ElympicsPlayPad.Auth (859a514)
* Restore the lobby WebSocket in ElympicsPlayPad.Lobby (5c714a2)
* Strip the WebSocket lobby from WebGL builds by default (b01c181)


### Code Refactoring

* Move AsyncGameSample onto the lobby wrapper and launcher (ce0a1d1)
* Simplify lobby code after SDK cleanup (2516770)


### BREAKING CHANGES :warning:

* **Requires Elympics SDK 0.28.0.**
* **Lobby.** `ElympicsLobbyClient` and the `ElympicsLobby` prefab are removed. Use `PlayPadCommunicator.Lobby` (`IElympicsLobbyWrapper`, now public). The auth, lobby and Rooms types moved to the `ElympicsPlayPad.Auth`, `ElympicsPlayPad.Lobby` and `ElympicsPlayPad.Rooms` namespaces, and members that were obsolete are removed. `IElympicsLobbyWrapper.ConnectStandaloneEditorToElympicsAsync` is removed.
* **Matches** run on `ElympicsMatchLauncher`. After a match, game code returns to the boot scene with `ElympicsMatchLauncher.Instance.ReturnToBootAsync()`; nothing returns automatically, and `ElympicsLaunchDefaults.PostMatchTimeoutSeconds` is the backstop. Web-hosted games still call `Quit(QuitArgs)` themselves.
* **WebGL builds strip the WebSocket lobby** unless the `ELYMPICS_PLAYPAD_KEEP_LOBBY` scripting define is set. Without it, `PlayPadCommunicator.Lobby`, `IElympicsLobbyWrapper`, Rooms (`IRoom`, `IRoomsManager`, Room hooks) and the WebSocket session types are unavailable. Game code using them must go under `#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY` or be removed. Define `ELYMPICS_PLAYPAD_KEEP_LOBBY` for WebGL builds that need the in-game lobby (`LaunchMode.Lobby`).
* **Communicators.** The Leaderboard, Replay, Tournament and VirtualDeposit communicators are removed. `PlayPadCommunicator.VirtualDepositCommunicator` (`IExternalBlockChainCurrencyCommunicator`) is replaced by `PlayPadCommunicator.CoinCommunicator` (`IExternalCoinCommunicator`). `UserDepositCollection`, `VirtualDepositUpdated`, `VirtualDepositRemoved`, `DisplayDepositPopup`, `ShowOnRamp`, `GetVirtualDeposit`, `EnsureVirtualDeposit`, `GetConnectedWalletCurrencyBalance`, `GetWalletCurrencyBalance`, `CustomStandaloneBlockChainCurrencyCommunicatorBase`, `VirtualDepositInfo` and `WalletBalanceInfo` are removed.
* `IExternalGameStatusCommunicator.PlayGame` (and its `CustomStandaloneGameStatusCommunicatorBase` override) is removed.
* Web3 features and SmartContractService support are removed.
* **AsyncGameSample:** its lobby scene must be `ElympicsConfig`'s boot scene. It plays its own quick match and ignores the page's "Blocked" play status, which the page reports for games without a tournament.
