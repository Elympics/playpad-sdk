using System;

namespace ElympicsPlayPad.Lobby
{
    internal interface IAsyncEventsDispatcher
    {
        /// <summary>Runs <paramref name="action"/> on the main thread. Called from network threads.</summary>
        void Enqueue(Action action);
    }
}
