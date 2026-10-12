using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElympicsPlayPad.Tests.Runtime.Mocks
{
    public abstract class ElympicsMonoProviderBaseTest : ElympicsTestConfigProviderBase
    {
        protected abstract string SceneName { get; }

        /// <summary>Loads <see cref="SceneName"/> by its path, so it does not have to be (enabled) in Build Settings.</summary>
        protected AsyncOperation LoadTestScene()
        {
            var guids = AssetDatabase.FindAssets(SceneName + " t:Scene");
            if (guids.Length != 1)
                throw new ArgumentException($"Expected exactly 1 {SceneName} scene asset, found {guids.Length}.");
            return EditorSceneManager.LoadSceneAsyncInPlayMode(AssetDatabase.GUIDToAssetPath(guids[0]), new LoadSceneParameters(LoadSceneMode.Single));
        }

        protected static IEnumerator WaitUntilOrFail(Func<bool> condition, string description, float timeoutSeconds = 10)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail($"Timed out after {timeoutSeconds} s waiting for {description}.");
                yield return null;
            }
        }
    }
}
