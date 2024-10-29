using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    public abstract class CustomStandaloneTournamentCommunicatorBase : MonoBehaviour, IExternalTournamentCommunicator
    {
        public abstract TournamentInfo? CurrentTournament { get; }
        public event Action<TournamentInfo> TournamentUpdated;
        public abstract UniTask<TournamentInfo?> GetTournament();
    }
}
