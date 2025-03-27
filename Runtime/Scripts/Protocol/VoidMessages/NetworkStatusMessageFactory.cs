#nullable enable

using System;
using System.Collections.Generic;
using Elympics.AssemblyCommunicator.Events;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    /// <summary>Allows createion of <see cref="NetworkStatusMessage"/> instances with the same size without unnecessary heap allocations.</summary>
    internal readonly struct NetworkStatusMessageFactory
    {
        private readonly RttReceived[] _dataArray;

        /// <summary>
        /// Creates a new factory that is guaranteed to not allocate any heap memory when calling
        /// <see cref="CreateMessage(Guid, IReadOnlyCollection{RttReceived})"/> on it for a collection with <paramref name="expectedDataCount"/> elements.
        /// </summary>
        public NetworkStatusMessageFactory(int expectedDataCount) => _dataArray = new RttReceived[expectedDataCount];

        public NetworkStatusMessage CreateMessage(Guid matchId, IReadOnlyList<RttReceived> data)
        {
            //Reuse array if possible
            var dataArray = _dataArray.Length == data.Count ? _dataArray : new RttReceived[data.Count];

            for (var i = 0; i < dataArray.Length; i++)
                dataArray[i] = data[i];

            return new() { matchId = matchId.ToString(), data = dataArray };
        }
    }
}
