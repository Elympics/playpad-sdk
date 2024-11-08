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
        public readonly CancellationTokenSource TimeOutCts;
        public CancellationTokenSource? Linked;
        public TicketStatus(CancellationTokenSource timeOutCts) => TimeOutCts = timeOutCts;
        public void Dispose()
        {
            TimeOutCts.Dispose();
            Linked?.Dispose();
        }
    }
}
