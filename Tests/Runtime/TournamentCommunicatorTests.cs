using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.ElympicsSystems.Internal;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.Runtime
{
    public class TournamentCommunicatorTests
    {
        [UnityTest]
        public IEnumerator WebGLTournamentCommunicator_GetRollingTournamentHistory_NullResponseShouldResultInNoArgumentNullExceptions() => UniTask.ToCoroutine(async () =>
        {
            var blockChainCurrencyCommunicator = Substitute.For<IExternalBlockChainCurrencyCommunicator>();
            var jsCommunicator = Substitute.For<IJsCommunicator>();
            _ = jsCommunicator.SendRequestMessage<GetRollingTournamentHistoryRequest, GetRollingTournamentHistoryResponse>(null!, null, CancellationToken.None)
                .ReturnsForAnyArgs(UniTask.FromResult(new GetRollingTournamentHistoryResponse
                {
                    entries = null,
                }));
            var communicator = new WebGLTournamentCommunicator(new ElympicsLoggerContext(Guid.Empty), blockChainCurrencyCommunicator, jsCommunicator);

            var result = await communicator.GetRollingTournamentHistory(uint.MaxValue);

            Assert.That(result.Entries, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator WebGLTournamentCommunicator_GetRollingTournamentHistory_EmptyResponseShouldResultInNoArgumentNullExceptions() => UniTask.ToCoroutine(async () =>
        {
            var blockChainCurrencyCommunicator = Substitute.For<IExternalBlockChainCurrencyCommunicator>();
            var jsCommunicator = Substitute.For<IJsCommunicator>();
            _ = jsCommunicator.SendRequestMessage<GetRollingTournamentHistoryRequest, GetRollingTournamentHistoryResponse>(null!, null, CancellationToken.None)
                .ReturnsForAnyArgs(UniTask.FromResult(new GetRollingTournamentHistoryResponse
                {
                    entries = Array.Empty<GetRollingTournamentHistoryResponse.HistoryEntry>(),
                }));
            var communicator = new WebGLTournamentCommunicator(new ElympicsLoggerContext(Guid.Empty), blockChainCurrencyCommunicator, jsCommunicator);

            var result = await communicator.GetRollingTournamentHistory(uint.MaxValue);

            Assert.That(result.Entries, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator WebGLTournamentCommunicator_GetRollingTournamentHistory_TestResponseShouldResultInNoArgumentNullExceptions() => UniTask.ToCoroutine(async () =>
        {
            var blockChainCurrencyCommunicator = Substitute.For<IExternalBlockChainCurrencyCommunicator>();
            _ = blockChainCurrencyCommunicator.ElympicsCoins.Returns(new Dictionary<Guid, CoinInfo>
            {
                { Guid.Parse("6b3676f7-6de2-4c43-bd23-581a9444445a"), new CoinInfo() },
            });
            var jsCommunicator = Substitute.For<IJsCommunicator>();
            _ = jsCommunicator.SendRequestMessage<GetRollingTournamentHistoryRequest, GetRollingTournamentHistoryResponse>(null!, null, CancellationToken.None)
                .ReturnsForAnyArgs(UniTask.FromResult(new GetRollingTournamentHistoryResponse
                {
                    entries = new[]
                    {
                        new GetRollingTournamentHistoryResponse.HistoryEntry
                        {
                            state = "Live",
                            prizes = new[] { "2000000000000000000" },
                            coinId = "6b3676f7-6de2-4c43-bd23-581a9444445a",
                            entryFee = "900000000000000000",
                            numberOfPlayers = 4,
                            gameVersion = "rolls1",
                            scores = new []
                            {
                                new RollingTournamentScore
                                {
                                    state = "Finished",
                                    avatar = "https://hosting-meta.elympics.ai/avatars/FEDABEA4-FB2C-48A4-97DB-3DBCE9892A1D.png",
                                    nickname = "Agile Shark",
                                    matchEnded = "2025-09-11T09:52:19.921149Z",
                                    mine = true,
                                    score = 1348,
                                    position = 1,
                                    prize = "2000000000000000000",
                                },
                            },
                            unreadSettled = false,
                        },
                    },
                }));
            var communicator = new WebGLTournamentCommunicator(new ElympicsLoggerContext(Guid.Empty), blockChainCurrencyCommunicator, jsCommunicator);

            var result = await communicator.GetRollingTournamentHistory(uint.MaxValue);

            Assert.That(result.Entries, Is.Not.Null);
        });

        [UnityTest]
        public IEnumerator WebGLTournamentCommunicator_GetRollingTournamentHistory_ModeratelyEmptyResponseShouldResultInNoArgumentNullExceptions() => UniTask.ToCoroutine(async () =>
        {
            var blockChainCurrencyCommunicator = Substitute.For<IExternalBlockChainCurrencyCommunicator>();
            _ = blockChainCurrencyCommunicator.ElympicsCoins.Returns(new Dictionary<Guid, CoinInfo>
            {
                { Guid.Empty, new CoinInfo() },
            });
            var jsCommunicator = Substitute.For<IJsCommunicator>();
            _ = jsCommunicator.SendRequestMessage<GetRollingTournamentHistoryRequest, GetRollingTournamentHistoryResponse>(null!, null, CancellationToken.None)
                .ReturnsForAnyArgs(UniTask.FromResult(new GetRollingTournamentHistoryResponse
                {
                    entries = new[]
                    {
                        new GetRollingTournamentHistoryResponse.HistoryEntry
                        {
                            state = "Live",
                            coinId = Guid.Empty.ToString(),
                            scores = new[]
                            {
                                new RollingTournamentScore
                                {
                                    mine = true,
                                    state = "Finished",
                                    matchEnded = "2025-04-03T02:01:00.000000Z",
                                },
                            },
                        },
                    },
                }));
            var communicator = new WebGLTournamentCommunicator(new ElympicsLoggerContext(Guid.Empty), blockChainCurrencyCommunicator, jsCommunicator);

            var result = await communicator.GetRollingTournamentHistory(uint.MaxValue);

            Assert.That(result.Entries, Is.Not.Null);
        });
    }
}
