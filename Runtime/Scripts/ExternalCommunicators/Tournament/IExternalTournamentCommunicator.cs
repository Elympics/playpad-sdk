#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public interface IExternalTournamentCommunicator
    {
        TournamentInfo? CurrentTournament { get; }
        event Action<TournamentInfo>? TournamentUpdated;
        UniTask<TournamentInfo?> GetTournament(CancellationToken ct = default);
    }
}
