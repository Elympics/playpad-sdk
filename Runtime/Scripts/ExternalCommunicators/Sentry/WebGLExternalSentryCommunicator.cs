#nullable enable

using System;
using Elympics;
using Elympics.AssemblyCommunicator;
using Elympics.AssemblyCommunicator.Events;
using Elympics.Events;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    internal class WebGLExternalSentryCommunicator : IExternalSentryCommunicator, IElympicsObserver<RttReceived>, IElympicsObserver<ReceivedStatsUpdated>, IElympicsObserver<ElympicsStateChanged>, IElympicsObserver<ElympicsLogEvent>
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private readonly WebGLRoundTripTimeReporter _rttReporter;
        private readonly IElympicsMatchLauncher _launcher;
        private MatchLauncherState _launcherState;

        public WebGLExternalSentryCommunicator(PlayPadMessagingSystem playPadMessagingSystem, IElympicsMatchLauncher launcher)
        {
            _launcher = launcher;
            _launcherState = launcher.State;
            _launcher.StateChanged += OnLauncherStateChanged;
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
            _playPadMessagingSystem.SendVoidMessageStringified(VoidMessageTypes.BreadcrumbMessage, message);
        }

        private static bool BlockLog(LogLevel level) => level switch
        {
            LogLevel.Trace => false,
            LogLevel.Debug => false,
            LogLevel.Info => false,
            LogLevel.Warning => false,
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
        public void OnEvent(ElympicsLogEvent argument) => LogCaptured(argument.Json, argument.LogLevel);

        /// <summary>Flushes the tail of the RTT buffer of every match, including ones not started by the lobby wrapper.</summary>
        private void OnLauncherStateChanged(MatchLauncherState state)
        {
            var wasRunning = _launcherState == MatchLauncherState.Running;
            _launcherState = state;
            if (wasRunning && state != MatchLauncherState.Running)
                _rttReporter.FlushRttBuffer();
        }

        public void Dispose() => _launcher.StateChanged -= OnLauncherStateChanged;
    }
}
