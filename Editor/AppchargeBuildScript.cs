using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AppchargeBuildScript
{
    private static string BuildOutputPath = "";

    public static void BuildAndroid()
    {
        Debug.Log("AppchargeBuildScript.BuildAndroid called");
        BuildForPlatform(BuildTarget.Android, "Android");
    }

    public static void BuildiOS()
    {
        BuildForPlatform(BuildTarget.iOS, "iOS");
    }

    public static void BuildWebGL()
    {
        BuildForPlatform(BuildTarget.WebGL, "WebGL");
    }

    private static void BuildForPlatform(BuildTarget target, string platformName)
    {
        // Get build output path from command line argument or use default
        string[] args = Environment.GetCommandLineArgs();
        BuildOutputPath = "";
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildOutputPath" && i + 1 < args.Length)
            {
                BuildOutputPath = args[i + 1];
                break;
            }
        }
        
        if (string.IsNullOrEmpty(BuildOutputPath))
        {
            BuildOutputPath = Path.Combine(Application.dataPath, "..", "Builds", platformName);
        }
        
        // Ensure directory exists
        Directory.CreateDirectory(BuildOutputPath);
        
        // Get scenes from build settings
        List<string> enabledScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }
        
        if (enabledScenes.Count == 0)
        {
            Debug.LogError("No scenes found in build settings!");
            EditorApplication.Exit(1);
            return;
        }
        
        string[] scenes = enabledScenes.ToArray();
        
        Debug.Log($"Building for {platformName}...");
        Debug.Log($"Output path: {BuildOutputPath}");
        Debug.Log($"Scenes: {string.Join(", ", scenes)}");
        
        // Build player
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildOutputPath,
            target = target,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            Debug.Log($"Build output: {BuildOutputPath}");
            EditorApplication.Exit(0);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.LogWarning("Build cancelled or unknown result.");
            EditorApplication.Exit(1);
        }
    }
}

