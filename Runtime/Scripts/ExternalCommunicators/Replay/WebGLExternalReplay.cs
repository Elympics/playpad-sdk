using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;
using Elympics.SnapshotAnalysis.Retrievers;
using Elympics.SnapshotAnalysis.Serialization;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    internal class WebGLExternalReplay : IExternalReplayCommunicator, IWebMessageReceiver
    {
        public event Action ReplayRetrieved;
        private readonly Action<SnapshotAnalysisRetriever> _watchReplay;
        private bool _replayLaunched;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(WebGLExternalReplay))
            .WithMonitoringEnabled();
        private byte[] _currentRawReplay;
        private SnapshotAnalysisRetriever _snapshotAnalysisRetriever;
        /// <param name="watchReplay">Launches the replay. The lobby wrapper does it when it is compiled in, so its state is reported.</param>
        public WebGLExternalReplay(PlayPadMessagingSystem playPadMessagingSystem, IElympicsMatchLauncher launcher, Action<SnapshotAnalysisRetriever> watchReplay)
        {
            _watchReplay = watchReplay;
            launcher.StateChanged += state =>
            {
                if (state != MatchLauncherState.Idle || !_replayLaunched)
                    return;
                _replayLaunched = false;
                _snapshotAnalysisRetriever = null;
            };
            playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.SnapshotReplay);
        }

        public static void LaunchReplay(string gameId, SnapshotAnalysisRetriever retriever) =>
            ElympicsMatchLauncher.Current!.LaunchSnapshotReplayAsync(gameId, retriever, null, CancellationToken.None).ContinueWith(result =>
            {
                if (!result.Success)
                    ElympicsLogger.LogError(result.ToString());
            }).Forget();

        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.SnapshotReplay:
                    {
                        var snapshotReplay = JsonUtility.FromJson<SnapshotReplayMessage>(message.message);
                        var bytes = Convert.FromBase64String(snapshotReplay.replay);
                        using var ms = new MemoryStream(bytes);
                        var replay = SnapshotDeserializer.DeserializeSnapshots(ms);
                        _snapshotAnalysisRetriever = new PlayPadSnapshotRetriever(replay);

                        var replayVersion = _snapshotAnalysisRetriever.RetrieveInitData().GameVersion;
                        var currentVersion = ElympicsConfig.LoadCurrentElympicsGameConfig().GameVersion;

                        if (replayVersion != currentVersion)
                            logger.LogError($"Game version mismatch. Replay was recorded using game version {replayVersion} and current game version is {currentVersion}. Use a matching version of the game to watch this replay.");
                        else
                            ReplayRetrieved?.Invoke();

                        break;
                    }
                    default:
                        return;
                }
            }
            catch (Exception e)
            {
                logger.LogException(e);
            }

        }
        public void PlayReplay()
        {
            if (_snapshotAnalysisRetriever == null)
                throw new InvalidOperationException("No replay has been found.");
            _replayLaunched = true;
            _watchReplay(_snapshotAnalysisRetriever);
        }
        SnapshotAnalysisRetriever IExternalReplayCommunicator.SnapshotRetriever() => _snapshotAnalysisRetriever;
    }
}
