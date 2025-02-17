using System;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;

namespace ElympicsPlayPad.ExternalCommunicators.Sentry
{
    internal class WebGLExternalSentryCommunicator : IExternalSentryCommunicator, IElympicsLoggerClient
    {
        private JsCommunicator _jsCommunicator;

        public WebGLExternalSentryCommunicator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            ElympicsLogger.RegisterLoggerClient(this);
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
        public void Dispose() => ElympicsLogger.UnregisterLoggerClient(this);
    }
}
