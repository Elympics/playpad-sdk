#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Responses;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    internal class RequestMessageDispatcher
    {
        private readonly Dictionary<int, TicketStatus> _ticketStatus = new();
        private readonly TimeSpan _requestTimeOut;

        public RequestMessageDispatcher(IJsCommunicatorRetriever messageRetriever)
        {
            _requestTimeOut = TimeSpan.FromSeconds(10 * 60);
            messageRetriever.ResponseObjectReceived += OnResponseObjectReceived;
        }

        public void RegisterTicket(int ticket)
        {
            var added = _ticketStatus.TryAdd(ticket, new TicketStatus(new CancellationTokenSource(_requestTimeOut)));
            if (added is false)
                throw new ProtocolException($"Ticket {ticket} already exist in map.", string.Empty);
        }

        public async UniTask<TReturn> RequestUniTaskOrThrow<TReturn>(int ticket, CancellationToken ct)
            where TReturn : struct
        {
            if (_ticketStatus.TryGetValue(ticket, out var ticketStatus) is false)
                throw new ProtocolException($"Cannot find ticketStatus for Ticket: {ticket}", string.Empty);
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
                    throw new ProtocolException("Request reached timeout.", string.Empty);

                ct.ThrowIfCancellationRequested();
            }

            if (IsErrorResponse(ticketStatus, out var code))
            {
                Debug.Log($"[{nameof(RequestMessageDispatcher)}] found error in response: {ticket}");
                var errorMessage = GetErrorDescription(ticketStatus);
                ClearTicketStatus(ticket);
                throw new ResponseException(code, errorMessage);
            }
            var response = GetResponseData<TReturn>(ticketStatus);
            ClearTicketStatus(ticket);
            UniTask.ReturnToMainThread();
            return response!;
        }
        private void ClearTicketStatus(int ticket)
        {
            if (_ticketStatus.Remove(ticket, out var removedTicketStatus))
                removedTicketStatus.Dispose();
        }

        private static string GetErrorDescription(TicketStatus ticketStatus) => RequestErrors.GetErrorMessage(ticketStatus.Response!.status, ticketStatus.Response!.type) + $"{Environment.NewLine}Details: {ticketStatus.Response!.response}";
        private static bool IsErrorResponse(TicketStatus ticketStatus, out int code)
        {
            code = ticketStatus.Response!.status;
            return code != 0;
        }
        private void OnResponseObjectReceived(string responseObject)
        {
            Debug.Log($"[{nameof(RequestMessageDispatcher)}] Response received: {responseObject}");
            var response = JsonUtility.FromJson<ResponseMessage>(responseObject);

            if (_ticketStatus.TryGetValue(response.ticket, out var ticketStatus) is false)
            {
                Debug.LogError($"Did not found ticketStatus for {response.ticket}");
                return;
            }

            if (ticketStatus.Response != null)
            {
                Debug.LogError($"Status map already contains response {response}. Discarding message");
                return;
            }

            if (ticketStatus.Cancelled)
            {
                ClearTicketStatus(response.ticket);
                return;
            }

            ticketStatus.Response = response;
        }

        private static TReturn GetResponseData<TReturn>(TicketStatus ticketStatus)
            where TReturn : struct
        {
            if (string.IsNullOrEmpty(ticketStatus.Response!.response))
                if (typeof(TReturn) != typeof(EmptyPayload))
                    throw new ProtocolException($"Response data is null or empty.", ticketStatus.Response!.type);
                else
                    return default;
            try
            {
                var fromJsonObject = JsonUtility.FromJson<TReturn>(ticketStatus.Response!.response);
                return fromJsonObject;
            }
            catch (Exception)
            {
                throw new ProtocolException($"Failed to parse response data to {nameof(TReturn)}", ticketStatus.Response!.type);
            }
        }

        internal Dictionary<int, TicketStatus> TicketStatus => _ticketStatus;
        internal const string RequestTimeOutSecFieldName = nameof(_requestTimeOut);
        internal void Reset() => _ticketStatus.Clear();
    }
}
