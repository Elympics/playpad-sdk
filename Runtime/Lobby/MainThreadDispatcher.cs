using System;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;

namespace ElympicsPlayPad.Lobby
{
    /// <summary>Posts to UniTask's Update loop, which runs before any script's Update.</summary>
    internal sealed class MainThreadDispatcher : IAsyncEventsDispatcher
    {
        public static readonly MainThreadDispatcher Instance = new();

        public void Enqueue(Action action) => UniTask.Post(() =>
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                ElympicsLogger.LogException(e);
            }
        });
    }
}
