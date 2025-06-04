#nullable enable
using System;
using Elympics.SnapshotAnalysis.Retrievers;
namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    public interface IExternalReplayCommunicator
    {
        event Action? ReplayRetrieved;
        internal SnapshotAnalysisRetriever SnapshotRetriever();
        void PlayReplay();
    }
}
