using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Protocol.RequestResponse;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public abstract class CustomStandaloneTournamentCommunicatorBase : MonoBehaviour, IExternalTournamentCommunicator
    {
        public event Action<TournamentInfo> TournamentUpdated;
        public abstract UniTask<CanPlayTournamentResponse> CanPlayTournament();
    }
}
