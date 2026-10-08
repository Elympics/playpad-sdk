#nullable enable

using System.Collections;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.Tests.Runtime.Mocks;
using ElympicsPlayPad.Wrappers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests
{
    public class PlayPadCommunicatorPrefabTests : ElympicsTestConfigProviderBase
    {
        private const string PrefabPath = "Packages/com.daftmobile.elympics-playpad/Runtime/Prefabs/PlayPadCommunicator.prefab";
        private GameObject? _instance;

        protected override bool RequiresTestConfig => true;

        [UnitySetUp]
        protected IEnumerator RemoveSurvivingCommunicator()
        {
            if (PlayPadCommunicator.Instance != null)
                Object.Destroy(PlayPadCommunicator.Instance.gameObject);
            PlayPadCommunicator.Instance = null;
            yield return null;
        }

        [TearDown]
        protected void TearDown()
        {
            if (_instance != null)
                Object.Destroy(_instance);
            PlayPadCommunicator.Instance = null;
        }

        [UnityTest]
        public IEnumerator RealPrefabInitializesWithTheDefaultLobbyWrapper()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            _instance = Object.Instantiate(prefab);
            yield return null;

            Assert.That(PlayPadCommunicator.Instance, Is.Not.Null);
            Assert.That(PlayPadCommunicator.Instance!.gameObject, Is.SameAs(_instance));
            Assert.That(PlayPadCommunicator.Instance.Lobby, Is.InstanceOf<DefaultElympicsLobbyWrapper>());
            Assert.That(PlayPadCommunicator.Instance.Lobby.State, Is.EqualTo(ElympicsState.Disconnected));
            Assert.That(PlayPadCommunicator.Instance.GameStatusCommunicator, Is.Not.Null);
        }
    }
}
