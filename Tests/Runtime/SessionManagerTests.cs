using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Tests.Runtime.Mocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ElympicsPlayPad.Tests
{
    public class SessionManagerTests : ElympicsMonoBaseTest
    {
        private SessionManager _sut;
        private PlayPadCommunicator _communicator;
        private static readonly IExternalAuthenticator AuthMock = Substitute.For<IExternalAuthenticator>();
        private static readonly IExternalGameStatusCommunicator GameMock = Substitute.For<IExternalGameStatusCommunicator>();
        private static readonly IExternalBlockChainCurrencyCommunicator VirtualDepositMock = Substitute.For<IExternalBlockChainCurrencyCommunicator>();
        private static readonly IExternalTournamentCommunicator TournamentMock = Substitute.For<IExternalTournamentCommunicator>();
        private static readonly IPlayPadMessagingSystem PlaypadCommunicatorMock = Substitute.For<IPlayPadMessagingSystem>();
        public override string SceneName => "ElympicsSessionManagerTestScene";
        public override bool RequiresElympicsConfig => true;

        private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private const string Nickname = "nickname";
        private const string DefaultEnvironment = "Dev";
        private const string DefaultClosestRegion = "warsaw";

        #region PlayStatusDefaults

        private const PlayStatus Status = PlayStatus.Play;
        private const string Label = "TestPlay";

        #endregion

        private const string FakeJwt = @"{
   ""header"": {
     ""alg"": ""RS256"",
     ""typ"": ""JWT""
   },
   ""payload"": {
     ""nameid"": ""057f2883-b4b4-4cc6-895f-e1332da86567"",
     ""auth-type"": ""client-secret"",
     ""nbf"": 1718803982,
     ""exp"": 111,
     ""iat"": 1718803982
   },
   ""signature"": ""rX85CHYGCpo2V1J6hXRj0rRySi-n7qxjiuwS98P9zS6W-hfKHKsApWJQeLUZ4_0DCUr8AE-YdkbYESKwv6Jl5OuyHDH4QCIVuTkCVrbT4duCiopitcVqwNubQARpTc7lApDAxihAtmdVUuUwz26po2ntlgv-p_JdHqN1g5Uk3vr9miKDdBzvSwSWwN1NP2cGEvzqlAs3wHtw4GYZChX_RugjM-vppuovQMOkwxJ7IvQXV7kb00ucpj71u9EmTmQFN9RMnB8b4c5K7-kXCM-_L2PNAC6MZX2-OExNWklQtqTUD3oF-dJFRH4Hew_ZEgt_SBw37NWN1NSfT2q1wnXh0TDpFPPnZSqYUGNYl7mhOlLrPWNi5e4dpiawy-23760qDmj4kriyqOPcVCzWTbmcvcEe-ktwBIo9MNwYZvQCFJ7yZfsdVTlw7WdBO9_Kf6JZNVZ7Rc6jjCN3OPmCJShTLg7GbiHOp9Bl8637mXXV7GwTzqZxoyAvU9ysRyRXC3kMkUEew0oyAr8eCXU1k-8DIiK_AYdzAUIqSfgV74MwONqQtmrxbGx8kw_l4D15ha7vOMI0QoN9Tu62ElFBgwk2j-1ysH7_7D_sx-9wYD-gUUaOIgL2e71cLzxzzQ0RJYh984BE6RawW4-mzjiR3J8g9NYPRhT-911w-F_HGRTXCZ4""
 }";

        private string _jwtEncoded;

        [OneTimeSetUp]
        public void OneTimeSetup() => _jwtEncoded = EncodeJwtFromJson(FakeJwt);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene(SceneName);
            yield return new WaitUntil(() => Object.FindObjectOfType<SessionManager>() != null);
            _sut = Object.FindObjectOfType<SessionManager>();
            _communicator = PlayPadCommunicator.Instance;
            Assert.NotNull(_communicator);
            MockExternalCommunicator(_communicator, PlayPadCommunicator.ExternalAuthenticatorFieldName, AuthMock);
            MockExternalCommunicator(_communicator, PlayPadCommunicator.GameStatusCommunicatorFieldName, GameMock);
            MockExternalCommunicator(_communicator, PlayPadCommunicator.VirtualDepositCommunicatorFieldName, VirtualDepositMock);
            MockIPlaypadCommunicator(_sut, PlaypadCommunicatorMock);
            _ = PlaypadCommunicatorMock.Connect().Returns(UniTask.CompletedTask);
            _ = VirtualDepositMock.GetElympicsCoins(Arg.Any<CancellationToken>()).Returns(x =>
            {
                var toReturn = new Dictionary<Guid, CoinInfo>();
                return UniTask.FromResult((IReadOnlyDictionary<Guid, CoinInfo>)toReturn);
            });
            Assert.NotNull(_sut);
            _sut.Reset();
        }

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret() => UniTask.ToCoroutine(async () =>
        {
            // Prepare
            _ = AuthMock.InitializationMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(UniTask.FromResult(new HandshakeInfo(false,
                    Capabilities.Ethereum,
                    DefaultEnvironment,
                    DefaultClosestRegion,
                    FeatureAccess.Authentication,
                    LaunchMode.Lobby | LaunchMode.Gameplay)));

            _ = AuthMock.Authenticate().Returns(UniTask.FromResult(new AuthData(UserId, _jwtEncoded, Nickname, AuthType.ClientSecret)));

            var currentPlayStatus = new PlayStatusInfo
            {
                PlayStatus = PlayStatus.Play,
                LabelInfo = Label,
            };
            _ = GameMock.CanPlayGame(Arg.Any<bool>())
                .Returns(UniTask.FromResult(currentPlayStatus));
            _ = GameMock.CurrentPlayStatus
                .Returns(currentPlayStatus);

            // Test
            await _sut.AuthenticateFromExternalAndConnect();

            // Assert
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(DefaultEnvironment, currSess.Environment);
            Assert.IsNull(currSess.AccountWallet);
            Assert.IsNull(currSess.SignWallet);
            Assert.AreEqual(DefaultClosestRegion, currSess.ClosestRegion);
            Assert.True(currSess.Features.HasOnlyAuthentication());
            Assert.AreEqual((int)Status, (int)PlayPadCommunicator.Instance!.GameStatusCommunicator!.CurrentPlayStatus.PlayStatus);
        });

        [UnityTest]
        public IEnumerator ReAuthenticateOnRegionChanged() => UniTask.ToCoroutine(async () =>
        {
            // Prepare
            _ = AuthMock.InitializationMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(UniTask.FromResult(new HandshakeInfo(false,
                    Capabilities.Ethereum,
                    DefaultEnvironment,
                    DefaultClosestRegion,
                    FeatureAccess.Authentication,
                    LaunchMode.Lobby | LaunchMode.Gameplay)));

            _ = AuthMock.Authenticate().Returns(UniTask.FromResult(new AuthData(UserId, _jwtEncoded, Nickname, AuthType.ClientSecret)));

            // Test
            await _sut.AuthenticateFromExternalAndConnect();
            var sessionInfoUpdated = false;
            var sessionInfoFinished = false;
            _sut.StartSessionInfoUpdate += () => sessionInfoUpdated = true;
            _sut.FinishSessionInfoUpdate += () => sessionInfoFinished = true;
            const string newRegion = "tokio";
            AuthMock.RegionUpdated += Raise.Event<Action<string>>(newRegion);
            await UniTask.WaitUntil(() => sessionInfoFinished);

            // Assert
            Assert.IsTrue(sessionInfoUpdated);
            Assert.IsTrue(sessionInfoFinished);
            Assert.AreSame(newRegion, _sut.CurrentSession?.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator ReAuthenticateOnRegionChange_ManyRequests() => UniTask.ToCoroutine(async () =>
        {
            // Prepare
            _ = AuthMock.InitializationMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(UniTask.FromResult(new HandshakeInfo(false,
                    Capabilities.Ethereum,
                    DefaultEnvironment,
                    DefaultClosestRegion,
                    FeatureAccess.Authentication,
                    LaunchMode.Lobby | LaunchMode.Gameplay)));

            _ = AuthMock.Authenticate().Returns(UniTask.FromResult(new AuthData(UserId, _jwtEncoded, Nickname, AuthType.ClientSecret)));

            // Test
            await _sut.AuthenticateFromExternalAndConnect();
            var sessionInfoFinishedCount = 0;
            _sut.FinishSessionInfoUpdate += () => sessionInfoFinishedCount++;
            const string newRegion = "tokio";
            const string newRegion2 = "warsaw";
            const string newRegion3 = "dallas";
            AuthMock.RegionUpdated += Raise.Event<Action<string>>(newRegion);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2));
            AuthMock.RegionUpdated += Raise.Event<Action<string>>(newRegion2);
            AuthMock.RegionUpdated += Raise.Event<Action<string>>(newRegion3);

            await UniTask.WaitUntil(() => sessionInfoFinishedCount == 2, PlayerLoopTiming.Update, new CancellationTokenSource(TimeSpan.FromSeconds(4)).Token);

            // Assert
            Assert.AreSame(newRegion3, _sut.CurrentSession?.ClosestRegion);
        });

        private static string EncodeJwtFromJson(string json)
        {
            var jwtObject = JObject.Parse(json);

            var expireTime = DateTime.UtcNow.AddHours(1);
            var epochTimeSpan = expireTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var epochTime = (long)epochTimeSpan.TotalSeconds;
            var header = jwtObject["header"]!.ToString(Formatting.None);
            jwtObject!["payload"]!["exp"] = epochTime;
            var payload = jwtObject["payload"]!.ToString(Formatting.None);
            var signature = jwtObject["signature"]!.ToString();

            var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
            var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
            var encodedSignature = Base64UrlEncode(Encoding.UTF8.GetBytes(signature));

            return $"{encodedHeader}.{encodedPayload}.{encodedSignature}";
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Replace('+', '-'); // Replace '+' with '-'
            output = output.Replace('/', '_'); // Replace '/' with '_'
            output = output.TrimEnd('='); // Remove any trailing '='
            return output;
        }

        private static void MockExternalCommunicator<T>(PlayPadCommunicator playPad, string externalCommunicatorName, T mock)
        {
            var externalCommunicator = playPad.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name == externalCommunicatorName);
            Assert.NotNull(playPad);
            externalCommunicator!.SetValue(playPad, mock);
        }

        private static void MockIPlaypadCommunicator(SessionManager sessionManager, IPlayPadMessagingSystem messagingSystem)
        {
            var field = typeof(SessionManager).GetField(SessionManager.PlayPadMessagingSystem, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field!.SetValue(sessionManager, messagingSystem);
        }

        [TearDown]
        public void ResetSut()
        {
            AuthMock.ClearSubstitute();
            GameMock.ClearSubstitute();
            TournamentMock.ClearSubstitute();
            AuthMock.ClearSubstitute();
            GameMock.ClearSubstitute();
            TournamentMock.ClearSubstitute();
            _sut.Reset();
        }
    }
}
