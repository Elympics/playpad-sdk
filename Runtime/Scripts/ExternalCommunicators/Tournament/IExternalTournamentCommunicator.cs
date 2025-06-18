#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public interface IExternalTournamentCommunicator
    {
        TournamentInfo? CurrentTournament { get; }
        event Action<TournamentInfo>? TournamentUpdated;
        UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default);
        UniTask<TournamentFeeInfo?> GetRollTournamentsFee(TournamentFeeRequestInfo[] requestData, CancellationToken ct = default);
    }
}
