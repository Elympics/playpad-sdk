#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using NSubstitute;

namespace ElympicsPlayPad.Tests.Lobby.Common.MockSetup
{
    /// <summary>Every Launch*Async method forwards to the non-virtual <c>LaunchAsync</c>, so substitutes are set up and checked through it.</summary>
    internal static class MatchLauncherMockSetup
    {
        public static ElympicsMatchLauncher SetupLaunch<T>(this ElympicsMatchLauncher mock, Func<T, UniTask<ElympicsLaunchResult>> launch)
            where T : LaunchData
        {
            _ = mock.LaunchAsync(Arg.Any<string?>(), IsData<T>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>())
                .Returns(call => launch(call.ArgAt<T>(1)));
            return mock;
        }

        public static void ReceivedLaunch<T>(this ElympicsMatchLauncher mock, int count, string gameId, Func<T, bool>? match = null)
            where T : LaunchData =>
            _ = mock.Received(count).LaunchAsync(gameId, IsData(match), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>());

        public static void DidNotReceiveLaunch<T>(this ElympicsMatchLauncher mock)
            where T : LaunchData =>
            _ = mock.DidNotReceive().LaunchAsync(Arg.Any<string?>(), IsData<T>(), Arg.Any<ElympicsLaunchOptions?>(), Arg.Any<CancellationToken>());

        // Arg.Is takes an expression tree, which can't contain a declaration pattern.
        private static LaunchData? IsData<T>(Func<T, bool>? match = null)
            where T : LaunchData =>
            Arg.Is<LaunchData?>(data => data is T && (match == null || match((T)data!)));
    }
}
