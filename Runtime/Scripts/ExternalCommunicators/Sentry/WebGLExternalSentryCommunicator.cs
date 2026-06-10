#nullable enable

using System;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Events;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.VoidMessages;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    internal class WebGLExternalSentryCommunicator : IExternalSentryCommunicator, IElympicsObserver<RttReceived>, IElympicsObserver<ReceivedStatsUpdated>, IElympicsObserver<ElympicsStateChanged>, IElympicsObserver<ElympicsLogEvent>
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private readonly WebGLRoundTripTimeReporter _rttReporter;

        public WebGLExternalSentryCommunicator(PlayPadMessagingSystem playPadMessagingSystem)
        {
            _playPadMessagingSystem = playPadMessagingSystem;
            _rttReporter = new WebGLRoundTripTimeReporter(32, _playPadMessagingSystem);
            CrossAssemblyEventBroadcaster.AddObserver<RttReceived>(this);
            CrossAssemblyEventBroadcaster.AddObserver<ReceivedStatsUpdated>(this);
            CrossAssemblyEventBroadcaster.AddObserver<ElympicsStateChanged>(this);
            CrossAssemblyEventBroadcaster.AddObserver<ElympicsLogEvent>(this);
        }

        private void LogCaptured(string message, LogLevel level)
        {
            if (BlockLog(level))
                return;

            var data = new BreadcrumbMessage
            {
                level = (int)level,
                message = message,
            };

            _playPadMessagingSystem.SendVoidMessage<BreadcrumbMessage>(VoidMessageTypes.BreadcrumbMessage, data);
        }
        private static bool BlockLog(LogLevel level) => level switch
        {
            LogLevel.Log => false,
            LogLevel.Warning => true,
            LogLevel.Error => false,
            LogLevel.Exception => false,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };

        public void OnEvent(RttReceived argument) => _rttReporter.OnRttReceived(argument);
        public void OnEvent(ReceivedStatsUpdated stats) => _rttReporter.OnReceivedStatsUpdated(stats);
        public void OnEvent(ElympicsStateChanged argument)
        {
            if (argument.PreviousState == ElympicsState.PlayingMatch)
                _rttReporter.FlushRttBuffer();
        }
        public void OnEvent(ElympicsLogEvent argument) => LogCaptured(argument.Message, argument.LogLevel);
    }
}
