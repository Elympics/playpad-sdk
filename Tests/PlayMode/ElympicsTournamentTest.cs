#nullable enable
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Elympics.Tests;
using ElympicsPlayPad.Tournament;
using ElympicsPlayPad.Tournament.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.PlayMode
{
    [Category("ElympicsTournament")]
    [SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
    public class ElympicsTournamentTest : ElympicsMonoBaseTest
    {
        public override string SceneName => "ElympicsTournamentTestScene";
        private ElympicsTournament? _sut;
        private Guid _owner = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private Guid _participant1 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene(SceneName);
            yield return new WaitUntil(() => ElympicsTournament.Instance != null);
            _sut = ElympicsTournament.Instance;
            Assert.NotNull(_sut);
        }

        [UnityTest]
        public IEnumerator GetTournament_NotStarted() => UniTask.ToCoroutine(async () =>
        {
            var created = DateTimeOffset.Now.AddMinutes(1);
            var start = DateTimeOffset.Now.AddMinutes(2);
            var end = DateTimeOffset.Now.AddMinutes(3);
            var tournamentInfo = CreateTournament(created, start, end);
            await _sut!.Initialize(tournamentInfo);
            AssertPlayState(TournamentPlayState.NonPlayable);
        });

        [UnityTest]
        public IEnumerator GetTournament_NotStarted_WaitForStart() => UniTask.ToCoroutine(async () =>
        {
            var created = DateTimeOffset.Now.AddSeconds(1);
            var start = DateTimeOffset.Now.AddSeconds(2);
            var end = DateTimeOffset.Now.AddSeconds(3);
            var tournamentInfo = CreateTournament(created, start, end);
            await _sut!.Initialize(tournamentInfo);
            Assert.AreEqual((int)TournamentPlayState.NonPlayable, (int)_sut.PlayState);
            var startEventCalled = false;
            _sut.TournamentStarted += () => startEventCalled = true;
            await UniTask.WaitUntil(() => DateTimeOffset.Now > start);
            Assert.IsTrue(startEventCalled);
        });

        [UnityTest]
        public IEnumerator GetTournament_Started_WaitForFinish() => UniTask.ToCoroutine(async () =>
        {
            var created = DateTimeOffset.Now.AddSeconds(-10);
            var start = DateTimeOffset.Now.AddSeconds(-2);
            var end = DateTimeOffset.Now.AddSeconds(1);
            var tournamentInfo = CreateTournament(created, start, end);
            await _sut!.Initialize(tournamentInfo);
            var finishedEventCalled = false;
            _sut.TournamentFinished += () => finishedEventCalled = true;
            await UniTask.WaitUntil(() => DateTimeOffset.Now > end);
            Assert.IsTrue(finishedEventCalled);
        });

        [UnityTest]
        public IEnumerator GetTournament_NotStarted_WalkThroughEntireTournament() => UniTask.ToCoroutine(async () =>
        {
            var created = DateTimeOffset.Now.AddSeconds(-10);
            var start = DateTimeOffset.Now.AddSeconds(1);
            var end = DateTimeOffset.Now.AddSeconds(2);
            var tournamentInfo = CreateTournament(created, start, end);
            await _sut!.Initialize(tournamentInfo);
            var startEventCalled = false;
            var finishedEventCalled = false;
            _sut.TournamentStarted += () => startEventCalled = true;
            _sut.TournamentFinished += () => finishedEventCalled = true;
            await UniTask.WaitUntil(() => DateTimeOffset.Now > start);
            Assert.IsTrue(startEventCalled);
            Assert.IsFalse(finishedEventCalled);
            startEventCalled = false;
            finishedEventCalled = false;
            await UniTask.WaitUntil(() => DateTimeOffset.Now > end);
            Assert.IsTrue(finishedEventCalled);
            Assert.IsFalse(startEventCalled);
        });

        private TournamentInfo CreateTournament(DateTimeOffset created, DateTimeOffset start, DateTimeOffset end)
        {
            return new TournamentInfo
            {
                Id = default,
                LeaderboardCapacity = 5,
                Name = "Test",
                LockedReason = null,
                PrizePool = null,
                OwnerId = _owner,
                State = TournamentState.Created,
                StartDate = start,
                EndDate = end,
                TonDetails = null,
                EvmDetails = null
            };
        }

        private void AssertPlayState(TournamentPlayState expected) => Assert.AreEqual((int)expected, (int)_sut!.PlayState);
    }
}
