#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad
{
    /// <summary>Auth provider for <see cref="ExternalCommunicators.Authentication.Models.LaunchMode.Gameplay"/>: the web page matchmakes and hands the match over.</summary>
    internal class PlayPadCommunicatorInternal : ISessionManagerAuthProvider
    {
        public AuthData? AuthData { get; private set; }
        private readonly ElympicsGameConfig _gameConfig;
        private readonly Func<IElympicsMatchLauncher> _launcher;

        public PlayPadCommunicatorInternal(ElympicsGameConfig gameConfig, Func<IElympicsMatchLauncher> launcher)
        {
            _gameConfig = gameConfig;
            _launcher = launcher;
        }

        public UniTask<ElympicsLaunchResult> PlayMatchInternal(RoomMatchData matchData)
        {
            var userId = AuthData?.UserId ?? throw new InvalidOperationException("Cannot play a match before authentication.");
            return MatchLaunching.LaunchAsync(_launcher(), _gameConfig, matchData, userId);
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
