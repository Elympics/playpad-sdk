using System;
using Elympics.Cysharp.Threading.Tasks;
using ElympicsPlayPad.Lobby;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace ElympicsPlayPad.Tests.Lobby.Common.MockSetup
{
    internal static class AsyncEventsDispatcherMockSetup
    {
        private static readonly IAsyncEventsDispatcher Dispatcher = Substitute.For<IAsyncEventsDispatcher>();
        public static IAsyncEventsDispatcher CreateMockAsyncEventsDispatcher()
        {
            Dispatcher.ClearSubstitute();
            Dispatcher.When(x => x.Enqueue(Arg.Any<Action>())).Do(info =>
            {
                var action = (Action)info.Args()[0];
                ExecuteOnMainThread(action).Forget();
            });
            return Dispatcher;
        }

        private static async UniTaskVoid ExecuteOnMainThread(Action action)
        {
            await UniTask.SwitchToMainThread();
            action.Invoke();
        }
    }
}
