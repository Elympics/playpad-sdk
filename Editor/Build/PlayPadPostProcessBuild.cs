using System;
using System.IO;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Utility;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ElympicsPlayPad.Editor.Build
{
    public class PostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder => PostProcessOrders.PlayPadPostProcessOrder;

        private const string MetaDataFile = "_meta.json";
        private const string BuildFolder = "Build";

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            Console.WriteLine("Creating PlayPad metadata.");

            var config = ElympicsConfig.LoadCurrentElympicsGameConfig() ?? throw new ElympicsException("Elympics config not found");
            ;
            try
            {
                var version = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly();

                var metaData = new PlayPadMeta
                {
                    sdkVersion = version,
                    protocolVersion = JsCommunicator.ProtocolVersion,
                    gameVersion = config.GameVersion
                };

                var content = JsonUtility.ToJson(metaData);

                var dir = new DirectoryInfo(report.summary.outputPath);

                var fileName = string.Concat(dir.Name, MetaDataFile);
                var path = Path.Combine(dir.FullName, BuildFolder, fileName);
                using var outputFile = new StreamWriter(path, false);
                outputFile.WriteLine(content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
