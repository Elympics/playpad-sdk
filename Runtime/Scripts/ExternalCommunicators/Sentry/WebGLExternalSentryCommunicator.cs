#nullable enable

using System;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.ElympicsSystems.Internal;
using Elympics.Events;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    internal class WebGLExternalSentryCommunicator : IExternalSentryCommunicator, IElympicsObserver<RttReceived>, IElympicsObserver<ElympicsStateChanged>, IElympicsObserver<ElympicsLogEvent>
    {
        private readonly JsCommunicator _jsCommunicator;
        private readonly WebGLRoundTripTimeReporter _rttReporter;

        public WebGLExternalSentryCommunicator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _rttReporter = new(32, _jsCommunicator);
            CrossAssemblyEventBroadcaster.AddObserver<RttReceived>(this);
            CrossAssemblyEventBroadcaster.AddObserver<ElympicsStateChanged>(this);
            CrossAssemblyEventBroadcaster.AddObserver<ElympicsLogEvent>(this);
        }

        private void LogCaptured(string message, string time, ElympicsLoggerContext log, LogLevel level)
        {
            if (BlockLog(log, level))
                return;

            var data = new BreadcrumbMessage
            {
                level = (int)level,
                message = message,
                data = MetaData.FromElympicsLoggerContext(time, log),
            };

            _jsCommunicator.SendVoidMessage<BreadcrumbMessage>(VoidMessageTypes.BreadcrumbMessage, data);
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
                default:
                    break;
            }
            return false;
        }

        public void OnEvent(RttReceived argument) => _rttReporter.OnRttReceived(argument);
        public void OnEvent(ElympicsStateChanged argument)
        {
            if (argument.PreviousState == ElympicsState.PlayingMatch)
                _rttReporter.FlushRttBuffer();
        }
        public void OnEvent(ElympicsLogEvent argument) => LogCaptured(argument.Message, argument.Time, argument.Context, argument.LogLevel);
    }
}
