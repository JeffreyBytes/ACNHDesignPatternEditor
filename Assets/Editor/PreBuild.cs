using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PreBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        UnityEngine.Debug.Log("Setting compiler flags");
        var additionalLibraries = "";
        var compilerFlags = ""; 
        var linkerFlags = "";
        if (report.summary.platform == UnityEditor.BuildTarget.StandaloneWindows || report.summary.platform == UnityEditor.BuildTarget.StandaloneWindows64)
        {
            linkerFlags += "comdlg32.lib";
        }
        PlayerSettings.SetAdditionalIl2CppArgs("--generic-virtual-method-iterations=5 " + (additionalLibraries != "" ? "--additional-libraries \""+additionalLibraries+"\"" : "") + (compilerFlags != "" ? "--compiler-flags=\"" + compilerFlags + "\" " : "") + (linkerFlags != "" ? "--linker-flags=\"" + linkerFlags + "\"" : ""));
    }
}
