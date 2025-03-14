#nullable enable
using Elympics;
using Elympics.SnapshotAnalysis.Retrievers;
using ElympicsPlayPad.ExternalCommunicators.Replay;

namespace ElympicsPlayPad
{
    internal class PlayPadCommunicatorInternal : ILobby
    {
        private readonly IExternalReplayCommunicator? _externalReplayCommunicator;
        public PlayPadCommunicatorInternal(IExternalReplayCommunicator? externalReplayCommunicator) => _externalReplayCommunicator = externalReplayCommunicator;

        public SnapshotAnalysisRetriever? SnapshotAnalysisRetriever => _externalReplayCommunicator?.SnapshotRetriever();
    }
}
