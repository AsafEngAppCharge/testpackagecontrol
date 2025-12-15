using Appcharge.PaymentLinks.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidPostProcess : IPreprocessBuildWithReport
{
    #region Variables
    public int callbackOrder => 0;
    public static AndroidPostProcess Instance { get; private set; }
    private AppchargePrebuildEditor _appchargePrebuildEditor;
    #endregion

    #region Constructor 
    public AndroidPostProcess()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    #endregion

    #region Reporters
    public void OnPreprocessBuild(BuildReport report)
    {
        _appchargePrebuildEditor = new AppchargePrebuildEditor();
        
        if (report.summary.platform == BuildTarget.Android)
        {
            _appchargePrebuildEditor.OnPreprocessBuild();
            AssetDatabase.Refresh();
        }
    }
    #endregion
}
