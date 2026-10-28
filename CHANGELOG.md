## [4.0.0](https://github.com/Elympics/playpad-sdk/compare/v3.0.0...v4.0.0) (2026-10-28)


### Bug Fixes 🪲

* **ci:** Copy WebGLTemplates~ into the testing project ([96f0191](https://github.com/Elympics/playpad-sdk/commit/96f01913e2877d70e16142956cadf6b2215a15ee))
* **ci:** Restore ProjectSettings when Unity fails in run_with_defines.sh ([7d714ee](https://github.com/Elympics/playpad-sdk/commit/7d714ee7bd80685859f9e30f8b580a391cd9afb2))
* Deliver WebGL WebSocket text frames and accept an empty protocol ([050d1ea](https://github.com/Elympics/playpad-sdk/commit/050d1ea61d519b42924355231c64a81822e1b3c3))
* Don't throw in LobbyManager.OnDestroy before Start ran ([efe609a](https://github.com/Elympics/playpad-sdk/commit/efe609a23a626691396aa306807d561b5f5aa4dd))
* Free the JS WebSocket instance once a WebGL socket closes ([b10fc38](https://github.com/Elympics/playpad-sdk/commit/b10fc388fc8bdacd04bbc935203335a8dd84eaa4))
* Null the closed WebGL socket instead of deleting it ([f5957f1](https://github.com/Elympics/playpad-sdk/commit/f5957f18cc0e380adca669a147be39e1fbbfcf5b))
* Rename the WebSocket jslib globals so they don't clash with Unity Wire ([09fb87e](https://github.com/Elympics/playpad-sdk/commit/09fb87ee185236be992e9c383e9a3fbe49b39b00))
* Report WebSocket constructor errors as connect failures ([737543d](https://github.com/Elympics/playpad-sdk/commit/737543db3efe62782c5ad2ab4497dda87907d6f8))
* **samples:** Remove AsyncGame configs and scene overrides for the deleted standalone communicators ([8cd02e6](https://github.com/Elympics/playpad-sdk/commit/8cd02e6ccb0de2ba6e3f3f5a6591ccd657dc8186))
* Stamp lobby pings on the WebSocket thread and check the heartbeat on the player loop ([bf433eb](https://github.com/Elympics/playpad-sdk/commit/bf433ebac457e19c527a62252c98263668695d9c))
* Stop predicting the AsyncGame sample's GameplayManager on clients ([489c726](https://github.com/Elympics/playpad-sdk/commit/489c7261f24fdf9826fba9d9288164b6786b9e4c))
* Update the ElympicsLobby WebGL template to v0.5.1 ([0af5d93](https://github.com/Elympics/playpad-sdk/commit/0af5d9376f82367edccc43cb4c11e6fdd699a8b3))
* Update the WebGL template without an AssetDatabase refresh during builds and keep its GUIDs ([57f096b](https://github.com/Elympics/playpad-sdk/commit/57f096bf074c175a46eb08523a5f06bbbb1b04d0))

### Features

* **BREAKING:** Fail ElympicsLobby WebGL builds without an Elympics region ([a0f52f9](https://github.com/Elympics/playpad-sdk/commit/a0f52f9a94bd624d6109f661ef56404e36da0090))
* **BREAKING:** Require Unity 6.3 and Elympics 0.29.0 ([cf53054](https://github.com/Elympics/playpad-sdk/commit/cf53054d25c45f4cb37cdf872900609b0617e425))
* **BREAKING:** Own the WebSocket client and use ClientWebSocket outside WebGL ([5d5247c](https://github.com/Elympics/playpad-sdk/commit/5d5247c5bb1bfe44f6e19451b0dacbe15157f2e2))
* **BREAKING:** Key the stored client secret by Multiplayer Play Mode player ([cfa3502](https://github.com/Elympics/playpad-sdk/commit/cfa350215fd7564ada4ad42ca745375da0b9e39a))
* Add a define that stops automatic WebGL template updates ([1f32882](https://github.com/Elympics/playpad-sdk/commit/1f3288254c9c1b55eddc93054caa39f95f43f25b))
* Add StandaloneAuthenticationCommunicator, a guest-login mock authenticator for running outside PlayPad ([a7f3634](https://github.com/Elympics/playpad-sdk/commit/a7f3634336a2cb0ea79a3e12310e6355bf35fa12))
* **cli:** Add PlayPad WebGL template commands ([2a247c7](https://github.com/Elympics/playpad-sdk/commit/2a247c7881bb68781ee4b1f62f50e2462e1c020e))
* **cli:** Add playpad_upload_client_build and the client upload section of the Manage Games window ([c923982](https://github.com/Elympics/playpad-sdk/commit/c923982369d45c9330b9284b849a61277350dde5))
* **cli:** Add playpad_upload_streaming_assets and the StreamingAssets upload section of the Manage Games window ([d1a5f81](https://github.com/Elympics/playpad-sdk/commit/d1a5f8178dcd1d843c7249a0fa8cca6323ec6f14))
* List PlayPad's scripting defines in Project Settings > Elympics ([84163ce](https://github.com/Elympics/playpad-sdk/commit/84163ce038b20562e2c5ab4bbe5b6f6408e7aad0))
* Refresh outdated copies of the ElympicsLobby WebGL template ([0a47c83](https://github.com/Elympics/playpad-sdk/commit/0a47c8309e7c1249075562e51ec384c2a692cdf7))
* Update the ElympicsLobby WebGL template to v0.5.0 ([be7b42f](https://github.com/Elympics/playpad-sdk/commit/be7b42f584e6f5e935a4e869b27f9576b1e9fece))

### Performance Improvements 🚀

* Skip the message buffer for single-frame WebSocket messages ([e220aec](https://github.com/Elympics/playpad-sdk/commit/e220aec2e71ed75034d3ae1deffc8a75cc391eaa))

### Code Refactoring

* **BREAKING:** Reference the Elympics-named MessagePack assemblies ([72b7dc2](https://github.com/Elympics/playpad-sdk/commit/72b7dc20c271a7d2254f9e18cb89fddf0dd11915))
* **BREAKING:** Use the Elympics-namespaced UniTask ([c194435](https://github.com/Elympics/playpad-sdk/commit/c19443568b8f6cfa3ea0970bb1cd43aaf725ead0))

### Chores

* **BREAKING:** Remove the unused Nethereum plugin ([45026f9](https://github.com/Elympics/playpad-sdk/commit/45026f966dd2f102304fdb7751c72279a8decd6d))

### BREAKING CHANGES :warning:

* Unity versions older than 6000.3 are no longer supported.
* Games that used the Nethereum DLLs through PlayPad must vendor them.
* The HybridWebSocket namespace is now ElympicsPlayPad.WebSockets.
* Outside WebGL, a WebSocket can be connected only once: Connect throws once a connection was started, Close before Connect throws, and the initial state is Closed.
* Requires Elympics with the renamed MessagePack assemblies.
* Requires Elympics with the renamed UniTask.
* Client secrets stored under the old _clone_<n> keys aren't read any more.
* WebGL builds that use the ElympicsLobby template now fail when neither TEMPLATE_REGION nor the ELYMPICS_REGION template custom value (Player Settings > Resolution and Presentation) is set.

### Migration notes

- **Requires Unity 6.3 and Elympics 0.29.0**; declares `com.unity.ugui` 2.0.0 and `com.unity.modules.unitywebrequesttexture`.
- **Owns the WebSocket client:** namespace `ElympicsPlayPad.WebSockets` (was `HybridWebSocket`). Outside WebGL it now uses `ClientWebSocket`.
- **ElympicsLobby WebGL template v0.5.1:** a built template reads only `region` and `queue` from the page URL. Unmodified installed copies update automatically; customized copies keep their content (see `Tools/PlayPad/Reinstall WebGL Template`).
- **ElympicsLobby builds need a region:** set the `ELYMPICS_REGION` template custom value (Player Settings > Resolution and Presentation, or `unity command playpad_set_template_values --region <name>`), or the `TEMPLATE_REGION` environment variable. `elympics_list_regions` lists the regions. Builds with other templates are unaffected.
- **New CLI commands** (needs `com.unity.pipeline` 0.8.0-exp.1, now a dependency): `playpad_get_template_status`, `playpad_install_template` (`--force true` replaces a customized copy), `playpad_get_template_values`, `playpad_set_template_values` (`--environment`, `--region`, `--queue`). Errors follow Elympics' `ELYMPICS_<CODE>: … Fix: …` format.
- **Client build and StreamingAssets uploads moved here from Elympics:** `playpad_upload_client_build` and `playpad_upload_streaming_assets` use the Editor's Elympics login, require `--confirm true`, and upload only folders inside the project unless `--allow_external_path true`. The Manage Games window shows both uploads in its PlayPad sections.
- **Nethereum plugin removed** (unused since the Web3 removal in 3.0.0).
- **Uses the renamed UniTask and MessagePack assemblies.**
- **Stored client secrets per Multiplayer Play Mode player** (`Elympics/AuthToken_<PlayerName>`); the old `_clone_<n>` keys aren't read.


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
