#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad
{
    /// <summary>Auth provider for <see cref="ExternalCommunicators.Authentication.Models.LaunchMode.Gameplay"/>: the web page matchmakes and hands the match over.</summary>
    internal class PlayPadCommunicatorInternal : ISessionManagerAuthProvider
    {
        public AuthData? AuthData { get; private set; }
        private readonly string _gameId;
        private readonly bool _useWeb;
        private readonly Func<IElympicsMatchLauncher> _launcher;

        /// <param name="useWeb">Usually <see cref="ElympicsGameConfig.UseWeb"/>.</param>
        public PlayPadCommunicatorInternal(string gameId, bool useWeb, Func<IElympicsMatchLauncher> launcher)
        {
            _gameId = gameId;
            _useWeb = useWeb;
            _launcher = launcher;
        }

        public UniTask<ElympicsLaunchResult> PlayMatchInternal(FoundMatch matchData)
        {
            var userId = AuthData?.UserId ?? throw new InvalidOperationException("Cannot play a match before authentication.");
            return MatchLaunching.LaunchAsync(_launcher(), _gameId, _useWeb, matchData, userId);
        }

        public UniTask Authenticate(AuthData cachedData, string region, bool autoRetry)
        {
            Debug.Log("Authenticated with cached data.");
            AuthData = cachedData;
            return UniTask.CompletedTask;
        }

        public void SignOut() => AuthData = null;
        public bool IsAuthenticated => AuthData != null;
    }
}
