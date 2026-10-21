#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Elympics.Core.Logger;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    internal class RequestMessageDispatcher
    {
        private readonly TimeSpan _requestTimeOut;
        private readonly LoggerConfig _logger = ElympicsLogger.WithPlayPadSdkService()
            .WithClass(typeof(RequestMessageDispatcher))
            .WithMonitoringEnabled();

        public RequestMessageDispatcher()
        {
            _requestTimeOut = TimeSpan.FromSeconds(10 * 60);
        }

        /// <summary>
        /// Registers a ticket to await a response on.
        /// </summary>
        /// <param name="messageType">
        /// The request type. Recorded so a timeout can name the message that never
        /// came back — without it the failure reads "[] Request reached timeout."
        /// and says nothing about what to look at.
        /// </param>
        /// <param name="timeout">
        /// Per-request override of the default wait. Pass one for machine-to-machine
        /// requests that should fail fast; leave it null for anything that waits on a
        /// person (wallet confirmations, on-ramp and modal flows), which legitimately
        /// takes minutes.
        /// </param>
        public void RegisterTicket(int ticket, string messageType = "", TimeSpan? timeout = null)
        {
            var added = TicketStatus.TryAdd(ticket, new TicketStatus(new CancellationTokenSource(timeout ?? _requestTimeOut), messageType));
            if (added)
                return;

            var logger = _logger.WithMethodName();
            throw logger.LogExceptionAndReturn(new ProtocolException($"Ticket {ticket} already exist in map.", messageType));
        }

        public async UniTask<TReturn> RequestUniTaskOrThrow<TReturn>(int ticket, CancellationToken ct)
            where TReturn : struct
        {
            var logger = _logger.WithMethodName();
            if (!TicketStatus.TryGetValue(ticket, out var ticketStatus))
                throw logger.LogExceptionAndReturn(new ProtocolException($"Cannot find ticketStatus for Ticket: {ticket}", string.Empty));
            var token = ticketStatus.Timeout.Token;
            if (ct != default)
            {
                var linked = CancellationTokenSource.CreateLinkedTokenSource(ticketStatus.Timeout.Token, ct);
                ticketStatus.Linked = linked;
                token = linked.Token;
            }
            var isCancelled = await UniTask.WaitUntil(() => ticketStatus.Response != null, PlayerLoopTiming.Update, token).SuppressCancellationThrow();

            if (isCancelled)
            {
                var isTimeout = ticketStatus.Timeout.Token.IsCancellationRequested;
                ticketStatus.Cancelled = true;
                if (ticketStatus.Response != null)
                    ClearTicketStatus(ticket);

                if (isTimeout)
                    throw logger.LogExceptionAndReturn(
                        new ProtocolException($"Request reached timeout. No response for ticket {ticket}.", ticketStatus.MessageType));

                ct.ThrowIfCancellationRequested();
            }

            if (IsErrorResponse(ticketStatus, out var code))
            {
                logger.LogInfo($"Found error in ticket {ticket} error {code} type: {ticketStatus.Response?.type}");
                var errorMessage = GetErrorDescription(ticketStatus);
                ClearTicketStatus(ticket);
                throw new ResponseException(code, errorMessage);
            }
            var response = GetResponseData<TReturn>(ticketStatus, _logger);
            ClearTicketStatus(ticket);
            _ = UniTask.ReturnToMainThread();
            return response!;
        }
        private void ClearTicketStatus(int ticket)
        {
            if (TicketStatus.Remove(ticket, out var removedTicketStatus))
                removedTicketStatus.Dispose();
        }

        private static string GetErrorDescription(TicketStatus ticketStatus) => RequestErrors.GetErrorMessage(ticketStatus.Response!.status, ticketStatus.Response!.type) + $"{Environment.NewLine}Details: {ticketStatus.Response!.response}";
        private static bool IsErrorResponse(TicketStatus ticketStatus, out int code)
        {
            code = ticketStatus.Response!.status;
            return code != 0;
        }
        public void OnResponseObjectReceived(string responseMessageJson)
        {
            var logger = _logger.WithMethodName();
            Debug.Log($"[{nameof(RequestMessageDispatcher)}] Response received: {responseMessageJson}");
            var response = JsonUtility.FromJson<ResponseMessage>(responseMessageJson);

            if (!TicketStatus.TryGetValue(response.ticket, out var ticketStatus))
            {
                logger.LogError($"Did not found ticketStatus for ticket: {response.ticket} type: {response.type}");
                return;
            }

            if (ticketStatus.Response != null)
            {
                logger.LogError($"Status map already contains response {response.type}. Discarding message");
                return;
            }

            if (ticketStatus.Cancelled)
            {
                ClearTicketStatus(response.ticket);
                return;
            }

            ticketStatus.Response = response;
        }

        private static TReturn GetResponseData<TReturn>(TicketStatus ticketStatus, LoggerConfig loggerContext)
            where TReturn : struct
        {
            var logger = loggerContext.WithMethodName();
            if (string.IsNullOrEmpty(ticketStatus.Response!.response))
                if (typeof(TReturn) != typeof(EmptyPayload))
                    throw logger.LogExceptionAndReturn(new ProtocolException($"Response data is null or empty.", ticketStatus.Response!.type));
                else
                    return default;
            try
            {
                var fromJsonObject = JsonUtility.FromJson<TReturn>(ticketStatus.Response!.response);
                return fromJsonObject;
            }
            catch (Exception)
            {
                throw logger.LogExceptionAndReturn(new ProtocolException($"Failed to parse response data to {nameof(TReturn)}", ticketStatus.Response!.type));
            }
        }

        internal Dictionary<int, TicketStatus> TicketStatus { get; } = new();
        internal const string RequestTimeOutSecFieldName = nameof(_requestTimeOut);
        internal void Reset() => TicketStatus.Clear();
    }
}
