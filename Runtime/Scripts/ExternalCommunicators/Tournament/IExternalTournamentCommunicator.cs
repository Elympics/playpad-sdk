using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Protocol.RequestResponse;
using ElympicsPlayPad.Tournament.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public interface IExternalTournamentCommunicator
    {
        event Action<TournamentInfo> TournamentUpdated;
        UniTask<CanPlayTournamentResponse> CanPlayTournament();

    }
}
