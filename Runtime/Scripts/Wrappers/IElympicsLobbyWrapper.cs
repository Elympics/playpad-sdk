#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.Auth.Models;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Session;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Wrappers
{
    /// <summary>Elympics lobby: authentication, the lobby connection and Rooms. Matches it starts run on <see cref="ElympicsMatchLauncher"/>.</summary>
    [PublicAPI]
    public interface IElympicsLobbyWrapper : ISessionManagerAuthProvider
    {
        event Action<ElympicsConnectionData>? ElympicsConnectionEstablished;
        event Action<ElympicsConnectionLostData>? ElympicsConnectionLost;

        ElympicsState State { get; }

        IRoomsManager RoomsManager { get; }

        AuthData? AuthData { get; }

        ElympicsUser? ElympicsUser { get; }

        IWebSocketSession WebSocketSession { get; }

        string CurrentRegion { get; }

        IReadOnlyCollection<string>? AvailableRegions { get; }

        IReadOnlyCollection<CoinInfo>? AvailableCoins { get; }

        /// <summary>If true, a match found in a room starts as soon as matchmaking completes.</summary>
        bool ShouldLoadGameplaySceneAfterMatchmaking { get; set; }

        UniTask ConnectToElympicsAsync(ConnectionData data);

        UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default);

        internal void WatchReplay(SnapshotAnalysisRetriever retriever);
    }
}
#endif
