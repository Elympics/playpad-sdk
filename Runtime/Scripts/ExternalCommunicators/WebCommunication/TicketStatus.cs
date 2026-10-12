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

        /// <summary>
        /// The request type this ticket was registered for. Kept so a timeout —
        /// which by definition has no response to read the type from — can still
        /// say WHICH message never came back.
        /// </summary>
        public readonly string MessageType;

        public TicketStatus(CancellationTokenSource timeout, string messageType)
        {
            Timeout = timeout;
            MessageType = messageType;
        }
        public void Dispose()
        {
            Timeout.Dispose();
            Linked?.Dispose();
        }
    }
}
