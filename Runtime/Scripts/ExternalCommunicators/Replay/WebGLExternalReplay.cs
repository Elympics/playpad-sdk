using System;
using System.IO;
using Elympics;
using Elympics.Core.Logger;
using Elympics.SnapshotAnalysis.Retrievers;
using Elympics.SnapshotAnalysis.Serialization;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Wrappers;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    internal class WebGLExternalReplay : IExternalReplayCommunicator, IWebMessageReceiver, IDisposable
    {
        public event Action ReplayRetrieved;
        private readonly ElympicsMatchLauncher _launcher;
        private readonly Action<SnapshotAnalysisRetriever> _watchReplay;
        private bool _replayLaunched;
        private static readonly LoggerConfig Logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(WebGLExternalReplay))
            .WithMonitoringEnabled();
        private SnapshotAnalysisRetriever _snapshotAnalysisRetriever;
        /// <param name="watchReplay">Launches the replay. The lobby wrapper does it when it is compiled in, so its state is reported.</param>
        public WebGLExternalReplay(PlayPadMessagingSystem playPadMessagingSystem, ElympicsMatchLauncher launcher, Action<SnapshotAnalysisRetriever> watchReplay)
        {
            _launcher = launcher;
            _watchReplay = watchReplay;
            _launcher.StateChanged += OnLauncherStateChanged;
            playPadMessagingSystem.RegisterIWebEventReceiver(this, WebMessageTypes.SnapshotReplay);
        }

        private void OnLauncherStateChanged(MatchLauncherState state)
        {
            if (state != MatchLauncherState.Idle || !_replayLaunched)
                return;
            _replayLaunched = false;
            _snapshotAnalysisRetriever = null;
        }

        public static void LaunchReplay(ElympicsMatchLauncher launcher, string gameId, SnapshotAnalysisRetriever retriever) =>
            launcher.LaunchSnapshotReplayAsync(gameId, retriever).ForgetLoggingFailure(Logger);

        public void OnWebMessage(WebMessage message)
        {
            var logger = Logger.WithMethodName();
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
                        _snapshotAnalysisRetriever = new InMemorySnapshotAnalysisRetriever(replay);

                        if (!SnapshotReplayLoader.TryCheckVersion(_snapshotAnalysisRetriever, ElympicsConfig.LoadCurrentElympicsGameConfig(), out var failure))
                            logger.LogError(failure.message);
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

        public void Dispose() => _launcher.StateChanged -= OnLauncherStateChanged;
    }
}
