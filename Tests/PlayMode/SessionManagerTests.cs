using System;
using System.Collections;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.Session;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public class SessionManagerTests : MonoBehaviour, IPrebuildSetup
    {
        public SessionManager _sut;
        private PlayPadCommunicator _communicator;
        private const string TestNameScene = "SessionManagerTestScene";
        private static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private const string Nickname = "nickname";
        private string _defaultEnvironment = "Dev";
        private string _walletAddress = "walletAddress";
        private string _chainId = "11155111";
        private string _defaultClosestRegion = "warsaw";

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

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SceneManager.LoadScene(TestNameScene);
            yield return new WaitUntil(() => FindObjectOfType<SessionManager>() != null);
            _sut = FindObjectOfType<SessionManager>();
            _communicator = PlayPadCommunicator.Instance;
            Assert.NotNull(_sut);
            _sut.Reset();
        }

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockExternalWalletCommunicatorAndSet(string.Empty, _chainId);
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Ethereum, _defaultEnvironment, _defaultClosestRegion);
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
        });

        [UnityTest] //TODO: Need to make IRegionSelector for the test purposes.
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret_NoClosestRegion() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockExternalWalletCommunicatorAndSet(string.Empty, _chainId);
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Ethereum, _defaultEnvironment, null);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.IsNull(currSess.AccountWallet);
            Assert.IsNull(currSess.SignWallet);
            Assert.AreEqual(ElympicsRegions.Warsaw, currSess.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_Eth() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Ethereum, _defaultEnvironment, _defaultClosestRegion);
            _communicator.MockExternalWalletCommunicatorAndSet(_walletAddress, _chainId);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.EthAddress == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_Telegram() => UniTask.ToCoroutine(async () =>
        {
            var jwt = EncodeJwtFromJson(FakeJwt);
            var auth = new AuthData(UserId, jwt, Nickname, AuthType.Telegram);
            _communicator.MockIExternalAuthenticatorAndSet(auth, Capabilities.Telegram, _defaultEnvironment, _defaultClosestRegion);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            Assert.IsNotNull(_sut.CurrentSession.Value.AuthData);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.Telegram == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Telegram == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator AuthenticateFromExternalAndConnect_ClientSecret_NoEthCapabilities() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Guest, _defaultEnvironment, _defaultClosestRegion);
            _communicator.MockExternalWalletCommunicatorAndSet(_walletAddress, _chainId);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Guest == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator AuthenticateAsEth_GetWalletDisconnectEvent_BecomeSecret() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Ethereum, _defaultEnvironment, _defaultClosestRegion);
            _communicator.MockExternalWalletCommunicatorWithDisconnectionAndSet(_walletAddress, _chainId);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.EthAddress == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            await _sut.TryReAuthenticateIfAuthDataChanged();
            currSess = _sut.CurrentSession.Value;
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
        });

        [UnityTest]
        public IEnumerator AuthenticateAsClient_GetWalletConnectedEvent_BecomeEth() => UniTask.ToCoroutine(async () =>
        {
            _communicator.MockIExternalAuthenticatorAndSet(null, Capabilities.Ethereum, _defaultEnvironment, _defaultClosestRegion);
            _communicator.MockExternalWalletCommunicatorWithConnectedAndSet(string.Empty, _chainId);
            await _sut.AuthenticateFromExternalAndConnect();
            Assert.IsNotNull(_sut.CurrentSession);
            var currSess = _sut.CurrentSession.Value;
            Assert.IsNotNull(currSess.AuthData);
            Assert.IsTrue(AuthType.ClientSecret == currSess.AuthData.AuthType);
            Assert.IsTrue(Capabilities.Ethereum == currSess.Capabilities);
            Assert.AreEqual(_defaultEnvironment, currSess.Environment);
            await _sut.TryReAuthenticateIfAuthDataChanged();
            currSess = _sut.CurrentSession.Value;
            Assert.IsTrue(AuthType.EthAddress == currSess.AuthData.AuthType);
            Assert.AreEqual(_defaultClosestRegion, currSess.ClosestRegion);
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

        public void Setup()
        {
#if UNITY_EDITOR
            Debug.Log($"Add test scene {TestNameScene} to build settings.");
            var currentScenes = EditorBuildSettings.scenes.ToList();
            if (currentScenes.Any(x => x.path.Contains(TestNameScene)))
                return;

            var guids = AssetDatabase.FindAssets(TestNameScene + " t:Scene");
            if (guids.Length != 1)
                throw new ArgumentException($"There cannot be more than 1 {TestNameScene} scene asset.");

            var scene = AssetDatabase.GUIDToAssetPath(guids[0]);
            var editorBuildSettingScene = new EditorBuildSettingsScene(scene, true);
            currentScenes.Add(editorBuildSettingScene);
            EditorBuildSettings.scenes = currentScenes.ToArray();
#endif
        }
    }
}
