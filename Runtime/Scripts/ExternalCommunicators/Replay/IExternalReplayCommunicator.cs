#nullable enable
using System;
using Elympics.SnapshotAnalysis.Retrievers;
namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    public interface IExternalReplayCommunicator
    {
        public event Action? ReplayRetrieved;
        public void PlayReplay();
        internal SnapshotAnalysisRetriever SnapshotRetriever();
    }
}
