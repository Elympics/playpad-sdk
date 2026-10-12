using System;
using Elympics;

namespace ElympicsPlayPad.Lobby
{
    internal sealed class MainThreadDispatcher : IAsyncEventsDispatcher
    {
        public static readonly MainThreadDispatcher Instance = new();

        public void Enqueue(Action action) => MainThread.Post(action);
    }
}
