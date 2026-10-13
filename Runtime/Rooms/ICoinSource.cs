#nullable enable

using System.Collections.Generic;
using System.Threading;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;

namespace ElympicsPlayPad.Rooms
{
    /// <summary>Coins and rolling tournament fees known to the lobby session.</summary>
    internal interface ICoinSource
    {
        IReadOnlyCollection<CoinInfo>? AvailableCoins { get; }

        UniTask<RollingsResponseDto?> GetRollTournamentsFeeInternal(TournamentFeeRequestInfo[] requestData, CancellationToken ct);
    }
}
