using System;
using System.IO;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;
using UnityEngine;
using Elympics.SnapshotAnalysis.Retrievers;
using Elympics.SnapshotAnalysis.Serialization;
using ElympicsPlayPad.Wrappers;

namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    internal class WebGLExternalReplay : IExternalReplayCommunicator, IWebMessageReceiver, IElympicsObserver<ElympicsStateChanged>
    {
        public event Action ReplayRetrieved;
        private readonly IElympicsLobbyWrapper _lobbyWrapper;
        private readonly ElympicsLoggerContext _logger;
        private byte[] _currentRawReplay;
        private SnapshotAnalysisRetriever _snapshotAnalysisRetriever;
        public WebGLExternalReplay(JsCommunicator jsCommunicator, ElympicsLoggerContext logger, IElympicsLobbyWrapper lobbyWrapper)
        {
            _lobbyWrapper = lobbyWrapper;
            CrossAssemblyEventBroadcaster.AddObserver(this);
            _logger = logger.WithContext(nameof(WebGLExternalReplay));
            jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.SnapshotReplay);
        }

        public void OnWebMessage(WebMessage message)
        {
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
                            _logger.Error($"Game version mismatch. Replay was recorded using game version {replayVersion} and current game version is {currentVersion}. Use a matching version of the game to watch this replay.");
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
                var logger = _logger.WithMethodName();
                logger.Exception(e);
            }

        }
        internal void PlayReplay()
        {
            if (_snapshotAnalysisRetriever == null)
                throw new InvalidOperationException("No replay has been found.");
            //_lobbyWrapper.WatchReplay(); TO DO: Add this once replays are not a developer only feature on PlayPad
        }
        SnapshotAnalysisRetriever IExternalReplayCommunicator.SnapshotRetriever() => _snapshotAnalysisRetriever;
        public void OnEvent(ElympicsStateChanged argument)
        {
            if (argument.PreviousState == ElympicsState.WatchReplay)
                _snapshotAnalysisRetriever = null;
        }
    }
}
