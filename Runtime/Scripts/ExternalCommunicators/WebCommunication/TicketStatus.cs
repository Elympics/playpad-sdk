#nullable enable
using System;
using System.Threading;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    internal class TicketStatus : IDisposable
    {
        public ResponseMessage? Response;
        public bool Cancelled;
        public readonly CancellationTokenSource Timeout;
        public CancellationTokenSource? Linked;
        public TicketStatus(CancellationTokenSource timeout) => Timeout = timeout;
        public void Dispose()
        {
            Timeout.Dispose();
            Linked?.Dispose();
        }
    }
}
