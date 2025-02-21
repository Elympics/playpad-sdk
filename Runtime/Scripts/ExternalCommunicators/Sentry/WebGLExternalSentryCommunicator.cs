#nullable enable

using System;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.WebMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    internal class WebGLExternalSentryCommunicator : IExternalSentryCommunicator, IElympicsLoggerClient, IElympicsObserver<RttReceived>
    {
        private readonly JsCommunicator _jsCommunicator;
        private readonly WebGLRoundTripTimeReporter _rttReporter;

        public WebGLExternalSentryCommunicator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _rttReporter = new(32, _jsCommunicator);
            ElympicsLogger.RegisterLoggerClient(this);
            ElympicsLobbyClient.Instance!.GameplaySceneMonitor.GameplayFinished += _rttReporter.FlushRttBuffer;
            CrossAssemblyEventBroadcaster.AddObserver(this);
        }

        public void LogCaptured(string message, string time, ElympicsLoggerContext log, LogLevel level)
        {
            if (BlockLog(log, level))
                return;

            var data = new BreadcrumbMessage
            {
                level = (int)level,
                message = message,
                data = MetaData.FromElympicsLoggerContext(time, log),
            };

            _jsCommunicator.SendVoidMessage<BreadcrumbMessage>(VoidEventTypes.BreadcrumbMessage, data);
        }
        private static bool BlockLog(ElympicsLoggerContext log, LogLevel level) => level switch
        {
            LogLevel.Log => BlockLogLevelStrategy(log),
            LogLevel.Warning => true,
            LogLevel.Error => false,
            LogLevel.Exception => false,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        private static bool BlockLogLevelStrategy(ElympicsLoggerContext log)
        {
            switch (log.Context)
            {
                case ElympicsLoggerContext.ElympicsContextApp:
                    if (log.MethodName == "Awake")
                    {
                        return true;
                    }
                    return false;
            }
            return false;
        }

        public void Dispose()
        {
            ElympicsLogger.UnregisterLoggerClient(this);
            ElympicsLobbyClient.Instance!.GameplaySceneMonitor.GameplayFinished -= _rttReporter.FlushRttBuffer;
            _rttReporter.Dispose();
        }

        public void OnEvent(RttReceived argument) => _rttReporter.OnRttReceived(argument);
    }
}
