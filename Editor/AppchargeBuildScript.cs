using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

public class AppchargeBuildScript
{
    private static string BuildOutputPath = "";

    public static void PreCompile()
    {
        Debug.Log("=== AppchargeBuildScript: Pre-compiling project ===");
        
        // Refresh asset database
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
        // Wait for compilation to complete
        Debug.Log("Waiting for script compilation to complete...");
        int waitCount = 0;
        while (EditorApplication.isCompiling && waitCount < 600) // Wait up to 60 seconds
        {
            System.Threading.Thread.Sleep(100);
            waitCount++;
            if (waitCount % 50 == 0)
            {
                Debug.Log($"Still compiling... ({waitCount * 100}ms)");
            }
        }
        
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("Script compilation did not complete within timeout!");
            EditorApplication.Exit(1);
            return;
        }
        
        Debug.Log("Script compilation completed successfully");
        
        // Check for compilation errors by trying to get assemblies
        try
        {
            var assemblies = CompilationPipeline.GetAssemblies();
            Debug.Log($"Found {assemblies.Length} compiled assemblies");
            
            // Try to access build settings to ensure Unity is ready
            var scenes = EditorBuildSettings.scenes;
            Debug.Log($"Found {scenes.Length} scenes in build settings");
            
            // Try to check if Android build target is supported
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("Android build target is not supported!");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log("All pre-compile checks passed");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during pre-compile checks: {ex.GetType().Name}: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            EditorApplication.Exit(1);
            return;
        }
        
        EditorApplication.Exit(0);
    }

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
        try
        {
            Debug.Log($"=== AppchargeBuildScript: Starting build for {platformName} ===");
            Debug.Log($"Unity version: {Application.unityVersion}");
            Debug.Log($"Platform: {Application.platform}");
            Debug.Log($"Is batch mode: {Application.isBatchMode}");
            
            // Wait for compilation to finish before building
            Debug.Log("Waiting for script compilation to complete...");
            int waitCount = 0;
            int maxWait = 600; // Wait up to 60 seconds in batch mode
            while (EditorApplication.isCompiling && waitCount < maxWait)
            {
                System.Threading.Thread.Sleep(100);
                waitCount++;
                if (waitCount % 50 == 0)
                {
                    Debug.Log($"Still compiling... ({waitCount * 100}ms)");
                }
            }
            
            if (EditorApplication.isCompiling)
            {
                Debug.LogError($"Script compilation did not complete within timeout ({maxWait * 100}ms)!");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log("Script compilation completed successfully");
            
            // Additional wait to ensure Unity is fully initialized in batch mode
            Debug.Log("Waiting for Unity to fully initialize...");
            System.Threading.Thread.Sleep(2000); // 2 second delay for batch mode
            
            // DO NOT call AssetDatabase.Refresh() here as it can cause reentrancy issues
            // Unity will refresh automatically when needed
            Debug.Log("Unity initialization complete");
            
            // For Android, disable signing requirement for automated builds
            if (target == BuildTarget.Android)
            {
                Debug.Log("Configuring Android build settings...");
                PlayerSettings.Android.useCustomKeystore = false;
                Debug.Log("Disabled custom keystore requirement for Android build");
            }
            
            // Get build output path from command line argument or use default
            string[] args = Environment.GetCommandLineArgs();
            BuildOutputPath = "";
            
            Debug.Log($"Command line args count: {args.Length}");
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildOutputPath" && i + 1 < args.Length)
                {
                    BuildOutputPath = args[i + 1];
                    Debug.Log($"Found buildOutputPath argument: {BuildOutputPath}");
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(BuildOutputPath))
            {
                BuildOutputPath = Path.Combine(Application.dataPath, "..", "Builds", platformName);
                Debug.Log($"Using default build output path: {BuildOutputPath}");
            }
            
            // Ensure directory exists
            Directory.CreateDirectory(BuildOutputPath);
            Debug.Log($"Created/verified build directory: {BuildOutputPath}");
            
            // Get scenes from build settings
            List<string> enabledScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }
            
            Debug.Log($"Found {enabledScenes.Count} enabled scenes in build settings");
            
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
            // Use Development build to avoid signing requirements for automated builds
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = BuildOutputPath,
                target = target,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
        
            Debug.Log("Starting BuildPipeline.BuildPlayer...");
            
            // Check if there are any compilation errors before building
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("Cannot build while scripts are compiling!");
                EditorApplication.Exit(1);
                return;
            }
            
            // Try to get compilation status
            try
            {
                var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
                Debug.Log($"Found {assemblies.Length} compiled assemblies");
                
                // Check for compilation issues
                foreach (var assembly in assemblies)
                {
                    if (assembly.compiledAssemblyReferences == null || assembly.allReferences == null)
                    {
                        Debug.LogWarning($"Assembly {assembly.name} has null references");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Could not check assemblies: {ex.Message}");
            }
            
            // Additional safety check - ensure Unity is ready
            Debug.Log("Performing final readiness checks...");
            
            // Check if we can access build settings
            try
            {
                var buildScenes = EditorBuildSettings.scenes;
                Debug.Log($"Build settings accessible. Scenes count: {buildScenes.Length}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Cannot access build settings: {ex.Message}");
                EditorApplication.Exit(1);
                return;
            }
            
            // Check if target platform is available
            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                Debug.LogError($"{platformName} build target is not supported!");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log($"{platformName} build target is supported");
            
            // Check for any compilation issues before building
            try
            {
                var compilationResult = CompilationPipeline.GetAssemblies();
                Debug.Log($"Compilation check: Found {compilationResult.Length} assemblies");
                
                // Check if any assemblies failed to compile
                foreach (var assembly in compilationResult)
                {
                    if (assembly == null)
                    {
                        Debug.LogWarning("Found null assembly in compilation result");
                        continue;
                    }
                    
                    // Check if assembly has issues
                    if (assembly.compiledAssemblyReferences == null)
                    {
                        Debug.LogWarning($"Assembly {assembly.name} has null compiledAssemblyReferences");
                    }
                    
                    if (assembly.allReferences == null)
                    {
                        Debug.LogWarning($"Assembly {assembly.name} has null allReferences");
                    }
                }
                
                Debug.Log("Assembly compilation check completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error checking compilation status: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
                // Don't fail here, continue with build attempt
            }
            
            // Additional check: Ensure Unity is fully ready
            // DO NOT call AssetDatabase.Refresh() here as it causes reentrancy issues
            Debug.Log("Waiting for Unity to be fully ready...");
            System.Threading.Thread.Sleep(1000); // Wait 1 second for any pending operations
            
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("Project is still compiling!");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log("Unity is ready for build");
            
            // Ensure Unity is in a clean state before building
            Debug.Log("Preparing Unity for build...");
            try
            {
                // Ensure we're not in play mode
                if (EditorApplication.isPlaying)
                {
                    Debug.LogError("Cannot build while in play mode!");
                    EditorApplication.Exit(1);
                    return;
                }
                
                Debug.Log("Unity is ready for build");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Warning during Unity preparation: {ex.Message}");
            }
            
            BuildReport report = null;
            try
            {
                Debug.Log("Calling BuildPipeline.BuildPlayer...");
                Debug.Log($"Build options: {buildPlayerOptions.options}");
                Debug.Log($"Build target: {buildPlayerOptions.target}");
                Debug.Log($"Output path: {buildPlayerOptions.locationPathName}");
                Debug.Log($"Scene count: {buildPlayerOptions.scenes.Length}");
                
                // Try to catch UnityException specifically
                try
                {
                    report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                }
                catch (UnityException uex)
                {
                    Debug.LogError($"UnityException during BuildPlayer: {uex.Message}");
                    Debug.LogError($"UnityException type: {uex.GetType().FullName}");
                    Debug.LogError($"Stack trace: {uex.StackTrace}");
                    
                    // Try to get more information about what failed
                    Debug.LogError("Attempting to get more error details...");
                    
                    // Re-throw to be caught by outer catch
                    throw;
                }
                
                if (report == null)
                {
                    Debug.LogError("BuildPipeline.BuildPlayer returned null report!");
                    EditorApplication.Exit(1);
                    return;
                }
                
                Debug.Log("BuildPipeline.BuildPlayer completed");
            }
            catch (UnityException uex)
            {
                Debug.LogError($"UnityException during BuildPlayer: {uex.Message}");
                Debug.LogError($"UnityException type: {uex.GetType().FullName}");
                Debug.LogError($"Stack trace: {uex.StackTrace}");
                if (uex.InnerException != null)
                {
                    Debug.LogError($"Inner exception: {uex.InnerException.GetType().Name}: {uex.InnerException.Message}");
                    Debug.LogError($"Inner stack trace: {uex.InnerException.StackTrace}");
                }
                EditorApplication.Exit(1);
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception during BuildPlayer: {ex.GetType().Name}: {ex.Message}");
                Debug.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    Debug.LogError($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                EditorApplication.Exit(1);
                return;
            }
            
            BuildSummary summary = report.summary;
            
            Debug.Log($"Build completed with result: {summary.result}");
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build output: {BuildOutputPath}");
                EditorApplication.Exit(0);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"Build failed! Total errors: {report.summary.totalErrors}, Total warnings: {report.summary.totalWarnings}");
                Debug.LogError($"Build duration: {report.summary.totalTime}");
                Debug.LogError($"Build size: {report.summary.totalSize} bytes");
                
                // Log all errors and warnings from build report
                if (report.steps != null)
                {
                    Debug.LogError($"=== Build Report: {report.steps.Length} steps ===");
                    foreach (var step in report.steps)
                    {
                        Debug.LogError($"Step: {step.name}, Duration: {step.duration}, Depth: {step.depth}");
                        
                        if (step.messages != null && step.messages.Length > 0)
                        {
                            Debug.LogWarning($"=== Build Step: {step.name} ({step.messages.Length} messages) ===");
                            foreach (var msg in step.messages)
                            {
                                if (msg.type == LogType.Error || msg.type == LogType.Exception)
                                {
                                    Debug.LogError($"[{step.name}] {msg.type}: {msg.content}");
                                    // Also log to console with more detail
                                    System.Console.Error.WriteLine($"[ERROR][{step.name}] {msg.content}");
                                }
                                else if (msg.type == LogType.Warning)
                                {
                                    Debug.LogWarning($"[{step.name}] {msg.content}");
                                }
                                else
                                {
                                    Debug.Log($"[{step.name}] {msg.content}");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Step {step.name} has no messages");
                        }
                    }
                }
                else
                {
                    Debug.LogError("Build report has no steps!");
                }
                
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.LogWarning($"Build cancelled or unknown result: {summary.result}");
                EditorApplication.Exit(1);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in BuildForPlatform: {ex.GetType().Name}: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            EditorApplication.Exit(1);
        }
    }
}

