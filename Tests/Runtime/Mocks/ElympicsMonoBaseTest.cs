using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Elympics;
using Elympics.Core.Logger;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ElympicsPlayPad.Tests.Runtime.Mocks
{
    public abstract class ElympicsMonoBaseTest : IPrebuildSetup
    {
        public abstract string SceneName { get; }
        public abstract bool RequiresElympicsConfig { get; }


        public void Setup()
        {
#if UNITY_EDITOR
            if (RequiresElympicsConfig)
            {
                ElympicsLogger.LogInfo("Setup configs");
                var config = ElympicsConfig.Load();
                if (!config)
                {
                    if (!Directory.Exists(ElympicsConfig.ElympicsResourcesPath))
                    {
                        ElympicsLogger.LogInfo("Creating Elympics resources directory...");
                        _ = Directory.CreateDirectory(ElympicsConfig.ElympicsResourcesPath);
                    }

                    var newConfig = ScriptableObject.CreateInstance<ElympicsConfig>();

                    const string resourcesDirectory = "Assets/Resources/";
                    var assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(resourcesDirectory + ElympicsConfig.PathInResources + ".asset");
                    AssetDatabase.CreateAsset(newConfig, assetPathAndName);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    config = newConfig;
                }
                var currentConfigs = Resources.LoadAll<ElympicsGameConfig>("Elympics");
                if (currentConfigs is null
                    || currentConfigs.Length == 0)
                {
                    var gameConfig = ScriptableObject.CreateInstance<ElympicsGameConfig>();
                    if (!Directory.Exists(ElympicsConfig.ElympicsResourcesPath))
                    {
                        ElympicsLogger.LogInfo("Creating Elympics Resources directory...");
                        _ = Directory.CreateDirectory(ElympicsConfig.ElympicsResourcesPath);
                        ElympicsLogger.LogInfo("Elympics Resources directory created successfully.");
                    }

                    AssetDatabase.CreateAsset(gameConfig, ElympicsConfig.ElympicsResourcesPath + "/ElympicsGameConfig.asset");
                    config.availableGames = new()
                    {
                        gameConfig,
                    };
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if (config.availableGames == null
                    || config.availableGames.Count == 0)
                {
                    var games = config.availableGames ?? new List<ElympicsGameConfig>();
                    games.Add(currentConfigs![0]);
                    config.availableGames = games;
                }
                ElympicsLogger.LogInfo($"Current test elympicsConfig has {config.availableGames.Count} games and current game ID is {config.GetCurrentGameConfig().GameId}");
            }
#endif
        }

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
