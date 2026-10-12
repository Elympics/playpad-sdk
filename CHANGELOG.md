## [3.0.0](https://github.com/Elympics/playpad-sdk/compare/v2.1.0...v3.0.0) (2026-10-12)


### Bug Fixes 🪲

* Assign the play button view in the async sample ([4905cc0](https://github.com/Elympics/playpad-sdk/commit/4905cc079009ca2cf15086f422c157748106b820))
* Correct "received" typo in the lobby status log ([d9e9b0c](https://github.com/Elympics/playpad-sdk/commit/d9e9b0c322357137b0bc95cd763ca5b4cd692b8e))
* Don't replay a finished web-lobby match ([775e50b](https://github.com/Elympics/playpad-sdk/commit/775e50b32470d85bbd5ccb14d2a5103852de1b99))
* dontdestroyonload for root object ([8ff7f05](https://github.com/Elympics/playpad-sdk/commit/8ff7f05f965da2ac618d813c3770fb173d9813c0))
* Follow the web host's match flow in the async sample ([506a153](https://github.com/Elympics/playpad-sdk/commit/506a1536dd935d3b3bf5bc0bd34e5c33c450648e))
* Import LaunchedMatch outside the lobby strip gate ([44fb8c6](https://github.com/Elympics/playpad-sdk/commit/44fb8c6c160aaf2ff89536d9031ffb4c7fc0ddef))
* Initialize the lobby wrapper before PlayPadCommunicator ([751fc57](https://github.com/Elympics/playpad-sdk/commit/751fc57f650f396116f32c891778595e9f3228e6))
* Keep only the coin catalog of the virtual-deposit communicator ([1a6471d](https://github.com/Elympics/playpad-sdk/commit/1a6471daf129ba3ec492c640a2c5c459575b072c))
* Launch lobby matches only from an idle launcher ([8d8888e](https://github.com/Elympics/playpad-sdk/commit/8d8888e8fe28a6bbd9de6366cfdce1f77a70cb91))
* Make PlayPad SDK compatible with Elympics SDK update ([f76421a](https://github.com/Elympics/playpad-sdk/commit/f76421abd03a3e2d3df4c47f806a1de1568a8867))
* Match the installed ElympicsLobby WebGL template in the build preprocessor ([1e5c129](https://github.com/Elympics/playpad-sdk/commit/1e5c1292c4a9988b37abf1d13e9edf6dc00c5f8d))
* Play quick matches in the async sample without the tournament feature ([e4e9615](https://github.com/Elympics/playpad-sdk/commit/e4e9615cecf9d8a0ddcdc8d7770e574490d844da))
* Raise web requests from JS as web requests ([07a4e0b](https://github.com/Elympics/playpad-sdk/commit/07a4e0bd829a04ce74aa365f70d2143466fa2e5e))
* Remove the unused ElympicsGetHref from the PlayPad jslib ([4dc3c85](https://github.com/Elympics/playpad-sdk/commit/4dc3c85b3c7f8d5249ec60db47914d3a26898a80))
* Show a working Play button in the async sample without a tournament ([07c2d7b](https://github.com/Elympics/playpad-sdk/commit/07c2d7be0e639e7140cfe32b33f86df3ce633a1c))
* Treat only a Gameplay-mode session as host-driven in the async sample ([663577c](https://github.com/Elympics/playpad-sdk/commit/663577c1d3e7ecd797d29e9e8622482d70346b0a))
* Use a test ElympicsConfig in tests ([abdd681](https://github.com/Elympics/playpad-sdk/commit/abdd6815a9d6af07e56f2fa289932155399a443e))


### Features

* Drive matches through ElympicsMatchLauncher ([a84e086](https://github.com/Elympics/playpad-sdk/commit/a84e086b0ebfbab7a3b854a363d3ae38e96461f5))
* Forward lost packet info in NetworkStatusMessages, add void messages for showing/hiding modals ([4902597](https://github.com/Elympics/playpad-sdk/commit/4902597ef74e5b92e5a2f3c136bac559d6e9be7b))
* Include warning-type logs in breadcrumbs ([6d5dca8](https://github.com/Elympics/playpad-sdk/commit/6d5dca874a27ba295092d0273a7224deff18d367))
* Restore Rooms in ElympicsPlayPad.Rooms ([8f1ea8d](https://github.com/Elympics/playpad-sdk/commit/8f1ea8dbc79531efec55a696d6d80b7628e97ad6))
* Restore the auth client in ElympicsPlayPad.Auth ([859a514](https://github.com/Elympics/playpad-sdk/commit/859a514803f00e911f97b791f982ba968d43e6e4))
* Restore the lobby WebSocket in ElympicsPlayPad.Lobby ([5c714a2](https://github.com/Elympics/playpad-sdk/commit/5c714a24c4b6c95ec91a1ec5c9fae7c3c2c276a5))
* Strip the WebSocket lobby from WebGL builds by default ([b01c181](https://github.com/Elympics/playpad-sdk/commit/b01c181fd1d030430ea2ce704e1df2b7c6216c53))


### Code Refactoring

* Move AsyncGameSample onto the lobby wrapper and launcher ([ce0a1d1](https://github.com/Elympics/playpad-sdk/commit/ce0a1d13082f8ec4339e5763340992e2151c7b4e))
* Simplify lobby code after SDK cleanup ([2516770](https://github.com/Elympics/playpad-sdk/commit/251677089020db609eebd6a49adf4cdd2acb67e4))


### BREAKING CHANGES :warning:

* **Requires Elympics SDK 0.28.0.**
* **Lobby.** `ElympicsLobbyClient` and the `ElympicsLobby` prefab are removed. Use `PlayPadCommunicator.Lobby` (`IElympicsLobbyWrapper`, now public). The auth, lobby and Rooms types moved to the `ElympicsPlayPad.Auth`, `ElympicsPlayPad.Lobby` and `ElympicsPlayPad.Rooms` namespaces, and members that were obsolete are removed. `IElympicsLobbyWrapper.ConnectStandaloneEditorToElympicsAsync` is removed.
* **Matches** run on `ElympicsMatchLauncher`. After a match, game code returns to the boot scene with `ElympicsMatchLauncher.Instance.ReturnToBootAsync()`; nothing returns automatically, and `ElympicsLaunchDefaults.PostMatchTimeoutSeconds` is the backstop. Web-hosted games still call `Quit(QuitArgs)` themselves.
* **WebGL builds strip the WebSocket lobby** unless the `ELYMPICS_PLAYPAD_KEEP_LOBBY` scripting define is set. Without it, `PlayPadCommunicator.Lobby`, `IElympicsLobbyWrapper`, Rooms (`IRoom`, `IRoomsManager`, Room hooks) and the WebSocket session types are unavailable. Game code using them must go under `#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY` or be removed. Define `ELYMPICS_PLAYPAD_KEEP_LOBBY` for WebGL builds that need the in-game lobby (`LaunchMode.Lobby`).
* **Communicators.** The Leaderboard, Replay, Tournament and VirtualDeposit communicators are removed. `PlayPadCommunicator.VirtualDepositCommunicator` (`IExternalBlockChainCurrencyCommunicator`) is replaced by `PlayPadCommunicator.CoinCommunicator` (`IExternalCoinCommunicator`). `UserDepositCollection`, `VirtualDepositUpdated`, `VirtualDepositRemoved`, `DisplayDepositPopup`, `ShowOnRamp`, `GetVirtualDeposit`, `EnsureVirtualDeposit`, `GetConnectedWalletCurrencyBalance`, `GetWalletCurrencyBalance`, `CustomStandaloneBlockChainCurrencyCommunicatorBase`, `VirtualDepositInfo` and `WalletBalanceInfo` are removed.
* `IExternalGameStatusCommunicator.PlayGame` (and its `CustomStandaloneGameStatusCommunicatorBase` override) is removed.
* Web3 features and SmartContractService support are removed.
* **AsyncGameSample:** its lobby scene must be `ElympicsConfig`'s boot scene. It plays its own quick match and ignores the page's "Blocked" play status, which the page reports for games without a tournament.
