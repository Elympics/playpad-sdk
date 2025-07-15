using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
namespace ElympicsPlayPad.ExternalCommunicators.Internal
{
    internal class WebGLHeartbeatCommunicator : IHeartbeatCommunicator
    {
        private readonly CancellationTokenSource _cts;
        private readonly JsCommunicator _jsCommunicator;
        private TimeSpan _heartbeatInterval;
        public WebGLHeartbeatCommunicator(JsCommunicator jsCommunicator)
        {
            _cts = new CancellationTokenSource();
            _jsCommunicator = jsCommunicator;
        }
        public void Dispose() => _cts?.Cancel();

        public void RunHeartbeat(ushort heartbeatIntervalMs)
        {
            if (heartbeatIntervalMs == 0)
                return;
            _heartbeatInterval = TimeSpan.FromMilliseconds(heartbeatIntervalMs);
            RunHeartbeatAsync().Forget();
        }

        private async UniTaskVoid RunHeartbeatAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await UniTask.Delay(_heartbeatInterval, DelayType.Realtime, PlayerLoopTiming.Update, _cts.Token);
                if (_cts.IsCancellationRequested)
                    break;
                _jsCommunicator.SendVoidMessage<EmptyPayload>(VoidMessageTypes.HeartbeatMessage);
            }
        }
    }
}
