#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Elympics.AssemblyCommunicator.Events;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    /// <summary>Allows createion of <see cref="NetworkStatusMessage"/> instances with the same size without unnecessary heap allocations.</summary>
    internal readonly struct NetworkStatusMessageFactory : IDisposable
    {
        private readonly byte[] _dataBuffer;
        private readonly BinaryWriter _writer;

        /// <summary>
        /// Creates a new factory that is guaranteed to not allocate any heap memory when calling
        /// <see cref="CreateMessage(Guid, IReadOnlyCollection{RttReceived})"/> on it for a collection with <paramref name="expectedDataCount"/> elements.
        /// </summary>
        public NetworkStatusMessageFactory(int expectedDataCount)
        {
            _dataBuffer = new byte[expectedDataCount * RttReceived.ByteSize];
            var stream = new MemoryStream(_dataBuffer);
            _writer = new BinaryWriter(stream);
        }

        public NetworkStatusMessage CreateMessage(Guid matchId, IReadOnlyCollection<RttReceived> data)
        {
            byte[] dataBuffer;
            BinaryWriter writer;

            var isDataCountExpected = _dataBuffer.Length == data.Count * RttReceived.ByteSize;

            //Reuse buffer if possible
            if (isDataCountExpected)
            {
                dataBuffer = _dataBuffer;
                writer = _writer;
            }
            else
            {
                dataBuffer = new byte[data.Count * RttReceived.ByteSize];
                var stream = new MemoryStream(dataBuffer);
                writer = new BinaryWriter(stream);
            }

            NetworkStatusMessage message;
            try
            {
                message = CreateMessage(matchId, data, dataBuffer, writer);
            }
            finally
            {
                if (isDataCountExpected)
                    ResetWrtier();
                else
                    writer.Dispose();
            }

            return message;
        }

        private void ResetWrtier() => _writer.Seek(0, SeekOrigin.Begin);

        private static NetworkStatusMessage CreateMessage(Guid matchId, IReadOnlyCollection<RttReceived> data, byte[] dataBuffer, BinaryWriter writer)
        {
            foreach (var entry in data)
                entry.Serialize(writer);

            return new() { matchId = matchId.ToString(), serializedData = dataBuffer };
        }

        public void Dispose() => ((IDisposable)_writer).Dispose();
    }
}
