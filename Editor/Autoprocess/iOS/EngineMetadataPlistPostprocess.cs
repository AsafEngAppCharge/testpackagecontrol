#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Appcharge.PaymentLinks.Editor;

namespace Appcharge.PaymentLinks.Editor
{
    /// <summary>
    /// Writes ACPaymentLinks.plist at iOS build time with ENGINE_NAME, ENGINE_VERSION_NAME, ENGINE_SDK_VERSION.
    /// Native iOS reads this; Unity runtime no longer passes these to init.
    /// </summary>
    public static class EngineMetadataPlistPostprocess
    {
        private const string PlistFileName = "ACPaymentLinks.plist";

        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
                return;

            string plistPath = Path.Combine(pathToBuiltProject, PlistFileName);
            var plist = new PlistDocument();
            plist.root.SetString("ENGINE_NAME", EngineMetadataBuild.EngineName);
            plist.root.SetString("ENGINE_VERSION_NAME", EngineMetadataBuild.EngineVersionName);
            plist.root.SetString("ENGINE_SDK_VERSION", EngineMetadataBuild.EngineSdkVersion);
            plist.WriteToFile(plistPath);

            // Add plist to Xcode project so it is copied into the app bundle
            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            if (File.Exists(projPath))
            {
                PBXProject proj = new PBXProject();
                proj.ReadFromFile(projPath);
#if UNITY_2019_3_OR_NEWER
                string mainTarget = proj.GetUnityMainTargetGuid();
#else
                string mainTarget = proj.TargetGuidByName("Unity-iPhone");
#endif
                string fileGuid = proj.AddFile(PlistFileName, PlistFileName, PBXSourceTree.Source);
                proj.AddFileToBuild(mainTarget, fileGuid);
                proj.WriteToFile(projPath);
            }
        }
    }
}
#endif
