using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using Elympics.Tests;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.JWT.Extensions;
using ElympicsPlayPad.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public class SessionManagerTests : ElympicsMonoBaseTest, IPrebuildSetup
    {
        public SessionManager _sut;
        private PlayPadCommunicator _communicator;
        public override string SceneName => "ElympicsSessionManagerTestScene";
        public override bool RequiresElympicsConfig => false;

        private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private const string Nickname = "nickname";
        private string _defaultEnvironment = "Dev";
        private string _defaultClosestRegion = "warsaw";

        #region TournamentDefaults

        private const string TournamentId = "abcdef";
        private const string TournamentName = "testname";
        private const int TournamentCapacity = 4;

        #endregion

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

        private string JwtEncoded;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            JwtEncoded = EncodeJwtFromJson(FakeJwt);
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene(SceneName);
            yield return new WaitUntil(() => MonoBehaviour.FindObjectOfType<SessionManager>() != null);
            _sut = MonoBehaviour.FindObjectOfType<SessionManager>();
            _communicator = PlayPadCommunicator.Instance;
            Assert.NotNull(_sut);
            _sut.Reset();
        }

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockInitializationMessage(Capabilities.Ethereum, FeatureAccess.Authentication, _defaultEnvironment, _defaultClosestRegion).MockAuthentication(UserId, JwtEncoded, Nickname, AuthType.ClientSecret, out _).MockPlayState(Status, Label);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.IsNull(currSess.AccountWallet);
            Assert.IsNull(currSess.SignWallet);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
            Assert.True(currSess.Features.HasOnlyAuthentication());
            Assert.AreEqual((int)Status, (int)PlayPadCommunicator.Instance.GameStatusCommunicator.CurrentPlayStatus.PlayStatus);
        });

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret_AuthChanged() => UniTask.ToCoroutine(async () =>
        {
            var nickNameUpdate = "NewNickName";
            _communicator.MockInitializationMessage(Capabilities.Ethereum, FeatureAccess.Authentication, _defaultEnvironment, _defaultClosestRegion).MockAuthentication(UserId, JwtEncoded, Nickname, AuthType.ClientSecret, out var authMock).MockPlayState(Status, Label);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var startSessionUpdateWasCalled = false;
            var finishSessionUpdateWasCalled = false;
            _sut.StartSessionInfoUpdate += () => startSessionUpdateWasCalled = true;
            _sut.FinishSessionInfoUpdate += () => finishSessionUpdateWasCalled = true;
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
            authMock.AuthenticationUpdated += Raise.Event<Action<AuthData>>(new AuthData(UserId, JwtEncoded, nickNameUpdate, AuthType.ClientSecret));
            await UniTask.WaitUntil(() => finishSessionUpdateWasCalled, PlayerLoopTiming.Update, cts.Token);
            Assert.IsNotNull(_sut.CurrentSession);
            Assert.AreEqual(nickNameUpdate, _sut.CurrentSession.Value.AuthData.Nickname);
            Assert.True(startSessionUpdateWasCalled);
            Assert.True(finishSessionUpdateWasCalled);
        });

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret_WithTournaments() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockInitializationMessage(Capabilities.Ethereum, FeatureAccess.Authentication | FeatureAccess.Tournament, _defaultEnvironment, _defaultClosestRegion).MockAuthentication(UserId, JwtEncoded, Nickname, AuthType.ClientSecret, out _).MockTournament(TournamentId, TournamentCapacity, TournamentName, DateTimeOffset.Now, DateTimeOffset.Now + TimeSpan.FromDays(1));
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.IsNull(currSess.AccountWallet);
            Assert.IsNull(currSess.SignWallet);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
            Assert.True(currSess.Features.HasAuthentication());
            Assert.True(currSess.Features.HasTournament());
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

        [TearDown]
        public void ResetSut()
        {
            _sut.Reset();
            _communicator.MockExternalWalletCommunicatorAndSet(string.Empty, string.Empty);
        }
    }
}
