using Appcharge.AppDirect.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Appcharge.AppDirect.PackageEditor
{
    public sealed class AndroidPostProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            new AppDirectPrebuildEditor().OnPreprocessBuild();
            AssetDatabase.Refresh();
        }
    }
}
