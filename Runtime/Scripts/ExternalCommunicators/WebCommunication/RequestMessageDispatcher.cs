#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication
{
    internal class RequestMessageDispatcher
    {
        private readonly ConcurrentDictionary<int, Response> _ticketStatus = new();
        private readonly List<int> _pendingTickets = new();
        private readonly object _lock = new();

        public RequestMessageDispatcher(IJsCommunicatorRetriever messageRetriever)
        {
            messageRetriever.ResponseObjectReceived += OnResponseObjectReceived;
        }

        public void RegisterTicket(int ticket)
        {
            Debug.Log($"[{nameof(RequestMessageDispatcher)}] Register pending ticket {ticket}");
            lock (_lock)
                _pendingTickets.Add(ticket);
        }

        public async UniTask<TReturn> RequestUniTaskOrThrow<TReturn>(int ticket)
        {
            Debug.Log($"[{nameof(RequestMessageDispatcher)}] Request task {ticket}");
            await UniTask.WaitUntil(() => IsWaitingForResponse(ticket) is false);
            if (IsErrorResponse(ticket, out var code))
            {
                Debug.Log($"[{nameof(RequestMessageDispatcher)}] found error in response: {ticket}");
                var errorMessage = GetErrorDescription(ticket);
                _ticketStatus.TryRemove(ticket, out _);
                throw new ResponseException(code, errorMessage);
            }
            var response = GetResponseData<TReturn>(ticket);
            _ticketStatus.TryRemove(ticket, out _);
            UniTask.ReturnToMainThread();
            return response;
        }

        private bool IsWaitingForResponse(int ticket) => _ticketStatus.ContainsKey(ticket) is false;

        private string GetErrorDescription(int ticket)
        {
            var status = _ticketStatus[ticket];
            return RequestErrors.GetErrorMessage(status.status, status.type) + $"{Environment.NewLine}Details: {_ticketStatus[ticket].response}";
        }
        private bool IsErrorResponse(int ticket, out int code)
        {
            code = _ticketStatus[ticket].status;
            return code != 0;
        }
        private void OnResponseObjectReceived(string responseObject)
        {
            Debug.Log($"[{nameof(RequestMessageDispatcher)}] Response received: {responseObject}");
            var response = JsonUtility.FromJson<Response>(responseObject);
            lock (_lock)
            {
                var isPending = _pendingTickets.Any(x => x == response.ticket);
                if (isPending is false)
                {
                    Debug.LogError($"Response {response} is not awaited by dispatcher. Discarding message.");
                    return;
                }
                _pendingTickets.Remove(response.ticket);
            }
            if (_ticketStatus.TryAdd(response.ticket, response) is false)
                Debug.LogError($"Status map already contains response {response}. Discarding message");
        }

        private TReturn? GetResponseData<TReturn>(int ticket)
        {
            var fetched = _ticketStatus.TryGetValue(ticket, out var responseData);
            if (!fetched)
                throw new ProtocolException($"No ticketStatus for {ticket} found in concurrent dictionary", string.Empty);

            if (string.IsNullOrEmpty(responseData!.response))
                throw new ProtocolException($"Response data is null or empty.", responseData.type);

            if (typeof(TReturn) == typeof(string))
                return (TReturn)(object)_ticketStatus[ticket].response;

            var fromJsonObject = JsonUtility.FromJson<TReturn>(_ticketStatus[ticket].response);
            if (fromJsonObject == null)
                throw new ProtocolException($"Failed to parse response data to {nameof(TReturn)}", responseData.type);

            return fromJsonObject;
        }
    }
}
