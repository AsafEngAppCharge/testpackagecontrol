#if UNITY_IOS
using Appcharge.PaymentLinks.Config;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Appcharge.PaymentLinks.Editor;

public static class iOSPostProcess
{
    private static PrebuildLogger _logger;

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) 
            return;

        var config = AssetDatabase.LoadAssetAtPath<AppchargeConfig>("Assets/Resources/Appcharge/AppchargeConfig.asset");
        
        if (config == null)
        {
            Debug.LogWarning("[Appcharge PostBuild] AppchargeConfig not found. Skipping post-build processing.");
            return;
        }

        if (!config.EnableIntegrationOptions)
            return;
        
        _logger = new PrebuildLogger();
        _logger.ClearLogs();
        _logger.Log("[Appcharge PostBuild] Starting iOS post-build processing...");
        Debug.Log("[Appcharge PostBuild] Starting iOS post-build processing...");

        if (config.AssociatedDomain != "")
            ProcessEntitlements(pathToBuiltProject, config.AssociatedDomain);
        
        if (config.AddFrameworkToXcodeProject)
            ProcessFramework(pathToBuiltProject);
        
        if (config.AddURLScheme)
            ProcessURLSchemes(pathToBuiltProject);

        if (config.EnableDebugMode)
            _logger.PrintLog();
    }

    private static void ProcessEntitlements(string pathToBuiltProject, string applinksDomain)
    {
        string entitlementFileName = "Appcharge.entitlements";
        string entitlementPath = Path.Combine(pathToBuiltProject, entitlementFileName);

        try
        {
            if (File.Exists(entitlementPath))
            {
                PlistDocument existingEntitlements = new PlistDocument();
                existingEntitlements.ReadFromFile(entitlementPath);
                
                string existingContent = existingEntitlements.WriteToString();
                _logger.Log($"Current entitlements file content:\n{existingContent}");
                
                if (existingEntitlements.root.values.ContainsKey("com.apple.developer.associated-domains"))
                {
                    PlistElementArray existingDomains = existingEntitlements.root.values["com.apple.developer.associated-domains"].AsArray();
                    bool domainExists = false;
                    string applinksDomainWithPrefix = "applinks:" + applinksDomain;
                    
                    for (int i = 0; i < existingDomains.values.Count; i++)
                    {
                        if (existingDomains.values[i].AsString() == applinksDomainWithPrefix)
                        {
                            domainExists = true; 
                            break;
                        }
                    }
                    
                    if (domainExists)
                    {
                        string finalContent = existingEntitlements.WriteToString();
                        _logger.Log($"Final entitlements file content:\n{finalContent}");
                        return;
                    }
                    else
                    {
                        existingDomains.AddString(applinksDomainWithPrefix);
                        File.WriteAllText(entitlementPath, existingEntitlements.WriteToString());
                        string message = $"[Appcharge PostBuild] Added applinks domain '{applinksDomain}' to existing entitlements";
                        _logger.Log(message);
                        Debug.Log(message);
                        string finalContent = existingEntitlements.WriteToString();
                        _logger.Log($"Final entitlements file content:\n{finalContent}");
                        return;
                    }
                }
                else
                {
                    PlistElementArray newDomains = existingEntitlements.root.CreateArray("com.apple.developer.associated-domains");
                    newDomains.AddString("applinks:" + applinksDomain);
                    File.WriteAllText(entitlementPath, existingEntitlements.WriteToString());
                    string message = $"[Appcharge PostBuild] Added applinks domain '{applinksDomain}' to existing entitlements";
                    _logger.Log(message);
                    Debug.Log(message);
                    string finalContent = existingEntitlements.WriteToString();
                    _logger.Log($"Final entitlements file content:\n{finalContent}");
                    return;
                }
            }

            PlistDocument entitlements = new PlistDocument();
            PlistElementArray domains = entitlements.root.CreateArray("com.apple.developer.associated-domains");
            domains.AddString("applinks:" + applinksDomain);

            File.WriteAllText(entitlementPath, entitlements.WriteToString());
            string createMessage = $"[Appcharge PostBuild] Created new entitlements file with applinks domain '{applinksDomain}'";
            _logger.Log(createMessage);
            Debug.Log(createMessage);
            string finalContentNew = entitlements.WriteToString();
            _logger.Log($"Final entitlements file content:\n{finalContentNew}");
        }
        catch (System.Exception e)
        {
            _logger.Log(e.Message, true);
            Debug.LogWarning("[Appcharge PostBuild] Failed to process entitlements: " + e.Message);
        }
    }

    private static void ProcessFramework(string pathToBuiltProject)
    {
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

        if (!File.Exists(projPath))
        {
            string errorMessage = $"[Appcharge PostBuild] Xcode project file not found at: {projPath}";
            _logger.Log(errorMessage, true);
            Debug.LogError(errorMessage);
            return;
        }

        try
        {
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

    #if UNITY_2019_3_OR_NEWER
            string mainTarget = proj.GetUnityMainTargetGuid();
            string unityFrameworkTarget = proj.GetUnityFrameworkTargetGuid();
    #else
            string mainTarget = proj.TargetGuidByName("Unity-iPhone");
            string unityFrameworkTarget = proj.TargetGuidByName("UnityFramework");
    #endif

            SetBuildPropertyIfDifferent(proj, unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            // Add framework search paths for both standard location and UPM package location
            AddFrameworkSearchPath(proj, mainTarget, "$(PROJECT_DIR)/Frameworks/Plugins/iOS");
            
            // Get the actual package path and calculate relative path for framework search
            string packagePath = GetPackagePath();
            if (!string.IsNullOrEmpty(packagePath))
            {
                string packagePluginsPath = Path.Combine(packagePath, "Runtime", "Plugins", "iOS");
                string relativePluginsPath = GetRelativePath(pathToBuiltProject, packagePluginsPath);
                if (!string.IsNullOrEmpty(relativePluginsPath))
                {
                    // Use relative path if it can be calculated
                    string frameworkSearchPath = "$(PROJECT_DIR)/" + relativePluginsPath.Replace("\\", "/");
                    AddFrameworkSearchPath(proj, mainTarget, frameworkSearchPath);
                }
                else
                {
                    // Fallback: use absolute path
                    AddFrameworkSearchPath(proj, mainTarget, packagePluginsPath);
                }
            }
            else
            {
                // Fallback: try standard Packages path
                AddFrameworkSearchPath(proj, mainTarget, "$(PROJECT_DIR)/Assets/Packages/com.appcharge.paymentlinks/Runtime/Plugins/iOS");
            }
            
            SetBuildPropertyIfDifferent(proj, mainTarget, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            SetBuildPropertyIfDifferent(proj, mainTarget, "SWIFT_VERSION", "5.0");
            SetBuildPropertyIfDifferent(proj, mainTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

            string entitlementFileName = "Appcharge.entitlements";
            string entitlementPath = Path.Combine(pathToBuiltProject, entitlementFileName);
            if (File.Exists(entitlementPath))
            {
                SetBuildPropertyIfDifferent(proj, mainTarget, "CODE_SIGN_ENTITLEMENTS", entitlementFileName);
            }

            SetBuildPropertyIfDifferent(proj, mainTarget, "CODE_SIGN_STYLE", "Automatic");

            AddXCFrameworkToProject(proj, pathToBuiltProject, mainTarget, "ACCheckoutSDK.xcframework");

            proj.WriteToFile(projPath);
            string message = "[Appcharge PostBuild] Framework embedding and project processing complete.";
            _logger.Log(message);
            Debug.Log(message);
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed to process Xcode project: {e.Message}";
            _logger.Log(errorMessage, true);
            Debug.LogWarning(errorMessage);
        }
    }

    private static void ProcessURLSchemes(string pathToBuiltProject)
    {
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        if (!File.Exists(plistPath))
        {
            string errorMessage = $"[Appcharge PostBuild] Info.plist not found at: {plistPath}";
            _logger.Log(errorMessage, true);
            Debug.LogError(errorMessage);
            return;
        }

        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        string initialContent = plist.WriteToString();
        _logger.Log($"Current Info.plist content:\n{initialContent}");

        const string URLTypesKey = "CFBundleURLTypes";
        PlistElementArray urlTypes = plist.root.values.ContainsKey(URLTypesKey)
            ? plist.root[URLTypesKey].AsArray()
            : plist.root.CreateArray(URLTypesKey);

        string urlIdentifier = "action";
        bool identifierExists = false;
        foreach (var urlType in urlTypes.values)
        {
            var dict = urlType.AsDict();
            if (dict.values.ContainsKey("CFBundleURLName") && dict["CFBundleURLName"].AsString() == urlIdentifier)
            {
                identifierExists = true;
                break;
            }
        }

        if (identifierExists)
        {
            string finalContent = plist.WriteToString();
            _logger.Log($"Final Info.plist content:\n{finalContent}");
            return;
        }

        PlistElementDict newURLType = urlTypes.AddDict();
        newURLType.SetString("CFBundleTypeRole", "Editor");
        newURLType.SetString("CFBundleURLName", urlIdentifier);
        PlistElementArray urlSchemes = newURLType.CreateArray("CFBundleURLSchemes");
        urlSchemes.AddString("acnative-$(PRODUCT_BUNDLE_IDENTIFIER)");

        plist.WriteToFile(plistPath);
        string successMessage = "[Appcharge PostBuild] Successfully added URL scheme 'acnative-$(PRODUCT_BUNDLE_IDENTIFIER)' to Info.plist.";
        _logger.Log(successMessage);
        Debug.Log(successMessage);
        string finalContentAdded = plist.WriteToString();
        _logger.Log($"Final Info.plist content:\n{finalContentAdded}");
    }

    private static void AddBuildPropertyIfNotExists(PBXProject proj, string target, string property, string value)
    {
        string existingValue = proj.GetBuildPropertyForAnyConfig(target, property);
        if (string.IsNullOrEmpty(existingValue))
        {
            proj.AddBuildProperty(target, property, value);
            Debug.Log($"[Appcharge PostBuild] Added {property} = {value} to target");
        }
    }

    private static void AddFrameworkSearchPath(PBXProject proj, string target, string path)
    {
        string existingValue = proj.GetBuildPropertyForAnyConfig(target, "FRAMEWORK_SEARCH_PATHS");
        if (string.IsNullOrEmpty(existingValue))
        {
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", path);
            Debug.Log($"[Appcharge PostBuild] Added FRAMEWORK_SEARCH_PATHS = {path} to target");
        }
        else if (!existingValue.Contains(path))
        {
            // Append the path if it doesn't already exist
            proj.AddBuildProperty(target, "FRAMEWORK_SEARCH_PATHS", path);
            Debug.Log($"[Appcharge PostBuild] Appended FRAMEWORK_SEARCH_PATHS with {path}");
        }
    }

    private static string GetPackagePath()
    {
        // Use Unity's PackageManager API to get the actual resolved path of the package
        try
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                .FirstOrDefault(p => p.name == "com.appcharge.paymentlinks");
            
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                return packageInfo.resolvedPath;
            }
        }
        catch (System.Exception e)
        {
            _logger?.Log($"Failed to get package path via PackageManager: {e.Message}", true);
            Debug.LogWarning($"[Appcharge PostBuild] Failed to get package path via PackageManager: {e.Message}");
        }
        
        return null;
    }

    private static string GetRelativePath(string fromPath, string toPath)
    {
        try
        {
            Uri fromUri = new Uri(Path.GetFullPath(fromPath) + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(Path.GetFullPath(toPath));
            
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            
            return relativePath;
        }
        catch (System.Exception e)
        {
            _logger?.Log($"Failed to calculate relative path: {e.Message}", true);
            return null;
        }
    }

    private static void SetBuildPropertyIfDifferent(PBXProject proj, string target, string property, string value)
    {
        string existingValue = proj.GetBuildPropertyForAnyConfig(target, property);
        if (existingValue != value)
        {
            proj.SetBuildProperty(target, property, value);
            Debug.Log($"[Appcharge PostBuild] Set {property} = {value} (was: {existingValue})");
        }
    }

    private static void AddXCFrameworkToProject(PBXProject proj, string pathToBuiltProject, string targetGuid, string xcframeworkName)
    {
        try
        {
            // Unity already includes the xcframework in the build, we just need to find it and add it to the target
            // Check common paths where Unity might have added it
            string[] possiblePaths = new string[]
            {
                Path.Combine("Packages", "com.appcharge.paymentlinks", "Runtime", "Plugins", "iOS", xcframeworkName).Replace("\\", "/"),
                Path.Combine("Frameworks", "Plugins", "iOS", xcframeworkName).Replace("\\", "/"),
                Path.Combine("Libraries", "Plugins", "iOS", xcframeworkName).Replace("\\", "/"),
                "Packages/com.appcharge.paymentlinks/Runtime/Plugins/iOS/" + xcframeworkName,
                "Frameworks/Plugins/iOS/" + xcframeworkName,
                "Libraries/Plugins/iOS/" + xcframeworkName,
                xcframeworkName
            };
            
            string fileGuid = null;
            foreach (string possiblePath in possiblePaths)
            {
                fileGuid = proj.FindFileGuidByProjectPath(possiblePath);
                if (!string.IsNullOrEmpty(fileGuid))
                {
                    _logger.Log($"[Appcharge PostBuild] Found existing xcframework reference at path: {possiblePath} with GUID: {fileGuid}");
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(fileGuid))
            {
                // If not found by path, search the PBX file directly
                string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                if (File.Exists(projPath))
                {
                    string pbxContent = File.ReadAllText(projPath);
                    
                    // Find file reference by name in comment: GUID /* ACCheckoutSDK.xcframework */
                    Regex commentPattern = new Regex(
                        $@"([A-F0-9]{{24}})\s*\/\*\s*{Regex.Escape(xcframeworkName)}\s*\*\/\s*=\s*{{isa\s*=\s*PBXFileReference;",
                        RegexOptions.IgnoreCase);
                    
                    Match match = commentPattern.Match(pbxContent);
                    if (match.Success)
                    {
                        fileGuid = match.Groups[1].Value;
                        _logger.Log($"[Appcharge PostBuild] Found existing xcframework reference by comment with GUID: {fileGuid}");
                    }
                }
            }
            
            if (string.IsNullOrEmpty(fileGuid))
            {
                string warningMessage = $"[Appcharge PostBuild] Could not find xcframework '{xcframeworkName}' in Xcode project. It may not have been included by Unity.";
                _logger.Log(warningMessage, true);
                Debug.LogWarning(warningMessage);
                return;
            }
            
            // Add to build phase and embed frameworks (only to main target)
            // Remove first to avoid duplicates, then add
            //proj.RemoveFileFromBuild(targetGuid, fileGuid);
            //proj.AddFileToBuild(targetGuid, fileGuid);
            proj.AddFileToEmbedFrameworks(targetGuid, fileGuid);
            
            string successMessage = $"[Appcharge PostBuild] Added '{xcframeworkName}' to Unity-iPhone target's Frameworks, Libraries and Embedded Content with Embed & Sign";
            _logger.Log(successMessage);
            Debug.Log(successMessage);
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed to add xcframework '{xcframeworkName}': {e.Message}";
            _logger.Log(errorMessage, true);
            Debug.LogWarning(errorMessage);
        }
    }
}
#endif