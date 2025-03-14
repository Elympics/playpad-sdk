using System.Collections.Generic;
using Elympics;
using Elympics.SnapshotAnalysis;
using Elympics.SnapshotAnalysis.Retrievers;
namespace ElympicsPlayPad
{
    internal class PlayPadSnapshotRetriever : SnapshotAnalysisRetriever
    {
        public PlayPadSnapshotRetriever(SnapshotReplayData replay) => Replay = replay;
        public override SnapshotSaverInitData RetrieveInitData() => Replay.InitData;
        public override Dictionary<long, ElympicsSnapshotWithMetadata> RetrieveSnapshots() => Replay.Snapshots;
    }
}
