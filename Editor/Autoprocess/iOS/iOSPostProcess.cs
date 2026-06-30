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

    // File names
    private const string ENTITLEMENTS_FILE_NAME = "Appcharge.entitlements";
    private const string INFO_PLIST_FILE_NAME = "Info.plist";
    private const string XCFRAMEWORK_NAME = "ACPaymentLinks.xcframework";
    
    // Build property names
    private const string BUILD_PROP_ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
    private const string BUILD_PROP_LD_RUNPATH_SEARCH_PATHS = "LD_RUNPATH_SEARCH_PATHS";
    private const string BUILD_PROP_SWIFT_VERSION = "SWIFT_VERSION";
    private const string BUILD_PROP_CODE_SIGN_ENTITLEMENTS = "CODE_SIGN_ENTITLEMENTS";
    private const string BUILD_PROP_CODE_SIGN_STYLE = "CODE_SIGN_STYLE";
    private const string BUILD_PROP_FRAMEWORK_SEARCH_PATHS = "FRAMEWORK_SEARCH_PATHS";
    
    // Build property values
    private const string BUILD_VALUE_NO = "NO";
    private const string BUILD_VALUE_YES = "YES";
    private const string BUILD_VALUE_EXECUTABLE_PATH_FRAMEWORKS = "@executable_path/Frameworks";
    private const string BUILD_VALUE_SWIFT_VERSION = "5.0";
    private const string BUILD_VALUE_CODE_SIGN_STYLE_AUTOMATIC = "Automatic";
    
    // Plist keys
    private const string PLIST_KEY_ASSOCIATED_DOMAINS = "com.apple.developer.associated-domains";
    private const string PLIST_KEY_CFBUNDLE_URL_NAME = "CFBundleURLName";
    private const string PLIST_KEY_CFBUNDLE_TYPE_ROLE = "CFBundleTypeRole";
    private const string PLIST_KEY_CFBUNDLE_URL_SCHEMES = "CFBundleURLSchemes";
    private const string PLIST_VALUE_TYPE_ROLE_EDITOR = "Editor";
    
    // URL scheme constants
    private const string APPLINKS_PREFIX = "applinks:";
    private const string URL_IDENTIFIER = "action";
    private const string URL_SCHEME_TEMPLATE = "acnative-$(PRODUCT_BUNDLE_IDENTIFIER)";
    
    // Path constants
    private const string PATH_FRAMEWORKS_PLUGINS_IOS = "$(PROJECT_DIR)/Frameworks/Plugins/iOS";
    private const string PATH_PACKAGES_PLUGINS_IOS = "$(PROJECT_DIR)/Assets/Packages/com.appcharge.paymentlinks/Runtime/Plugins/iOS";
    private const string PATH_SEGMENT_RUNTIME = "Runtime";
    private const string PATH_SEGMENT_PLUGINS = "Plugins";
    private const string PATH_SEGMENT_IOS = "iOS";
    private const string PATH_SEGMENT_PACKAGES = "Packages";
    private const string PATH_SEGMENT_FRAMEWORKS = "Frameworks";
    private const string PATH_SEGMENT_LIBRARIES = "Libraries";
    
    // Target names
    private const string TARGET_UNITY_IPHONE = "Unity-iPhone";
    private const string TARGET_UNITY_FRAMEWORK = "UnityFramework";
    
    // Package name
    private const string SDK_PACKAGE_NAME = "com.appcharge.paymentlinks";
    
    // Config path
    private const string CONFIG_ASSET_PATH = "Assets/Resources/Appcharge/AppchargeConfig.asset";

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS) 
            return;

        var config = AssetDatabase.LoadAssetAtPath<AppchargeConfig>(CONFIG_ASSET_PATH);
        
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

        if (config.EnableIOSEntitlementsIntegration && config.AssociatedDomain != "")
            ProcessEntitlements(pathToBuiltProject, config.AssociatedDomain, config);
        
        if (config.EnableIOSFrameworkIntegration)
            ProcessFramework(pathToBuiltProject, config);
        
        if (config.EnableIOSURLSchemeIntegration)
            ProcessURLSchemes(pathToBuiltProject, config);

        if (config.EnableDebugMode)
            _logger.PrintLog();
    }

    private static void ProcessEntitlements(string pathToBuiltProject, string applinksDomain, AppchargeConfig config)
    {
        string entitlementPath = Path.Combine(pathToBuiltProject, ENTITLEMENTS_FILE_NAME);

        try
        {
            if (File.Exists(entitlementPath))
            {
                PlistDocument existingEntitlements = new PlistDocument();
                existingEntitlements.ReadFromFile(entitlementPath);
                
                string existingContent = existingEntitlements.WriteToString();
                _logger.Log($"Current entitlements file content:\n{existingContent}");
                
                if (existingEntitlements.root.values.ContainsKey(PLIST_KEY_ASSOCIATED_DOMAINS))
                {
                    PlistElementArray existingDomains = existingEntitlements.root.values[PLIST_KEY_ASSOCIATED_DOMAINS].AsArray();
                    bool domainExists = false;
                    string applinksDomainWithPrefix = APPLINKS_PREFIX + applinksDomain;
                    
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
                        if (!config.ExcludeAddAssociatedDomain)
                        {
                            existingDomains.AddString(applinksDomainWithPrefix);
                            File.WriteAllText(entitlementPath, existingEntitlements.WriteToString());
                            string message = $"[Appcharge PostBuild] Added applinks domain '{applinksDomain}' to existing entitlements";
                            _logger.Log(message);
                            string finalContent = existingEntitlements.WriteToString();
                            _logger.Log($"Final entitlements file content:\n{finalContent}");
                        }
                        return;
                    }
                }
                else
                {
                    if (!config.ExcludeCreateAssociatedDomainsKey)
                    {
                        PlistElementArray newDomains = existingEntitlements.root.CreateArray(PLIST_KEY_ASSOCIATED_DOMAINS);
                        if (!config.ExcludeAddAssociatedDomain)
                        {
                            newDomains.AddString(APPLINKS_PREFIX + applinksDomain);
                        }
                        File.WriteAllText(entitlementPath, existingEntitlements.WriteToString());
                        string message = $"[Appcharge PostBuild] Added applinks domain '{applinksDomain}' to existing entitlements";
                        _logger.Log(message);
                        string finalContent = existingEntitlements.WriteToString();
                        _logger.Log($"Final entitlements file content:\n{finalContent}");
                    }
                    return;
                }
            }

            if (!config.ExcludeCreateEntitlementsFile)
            {
                PlistDocument entitlements = new PlistDocument();
                if (!config.ExcludeCreateAssociatedDomainsKey)
                {
                    PlistElementArray domains = entitlements.root.CreateArray(PLIST_KEY_ASSOCIATED_DOMAINS);
                    if (!config.ExcludeAddAssociatedDomain)
                    {
                        domains.AddString(APPLINKS_PREFIX + applinksDomain);
                    }
                }

                File.WriteAllText(entitlementPath, entitlements.WriteToString());
                string createMessage = $"[Appcharge PostBuild] Created new entitlements file with applinks domain '{applinksDomain}'";
                _logger.Log(createMessage);
                string finalContentNew = entitlements.WriteToString();
                _logger.Log($"Final entitlements file content:\n{finalContentNew}");
            }
        }
        catch (System.Exception e)
        {
            _logger.Log(e.Message, true);
            Debug.LogWarning("[Appcharge PostBuild] Failed to process entitlements: " + e.Message);
        }
    }

    private static void ProcessFramework(string pathToBuiltProject, AppchargeConfig config)
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
            string mainTarget = proj.TargetGuidByName(TARGET_UNITY_IPHONE);
            string unityFrameworkTarget = proj.TargetGuidByName(TARGET_UNITY_FRAMEWORK);
    #endif

            if (!config.ExcludeSetSwiftStandardLibrariesForFramework)
            {
                SetBuildPropertyIfDifferent(proj, unityFrameworkTarget, BUILD_PROP_ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES, BUILD_VALUE_NO);
            }

            // Add framework search paths for both standard location and UPM package location
            if (!config.ExcludeAddFrameworkSearchPaths)
            {
                AddFrameworkSearchPath(proj, mainTarget, PATH_FRAMEWORKS_PLUGINS_IOS);
                
                // Get the SDK package path and calculate relative path for framework search
                string sdkPackagePath = GetSDKPackagePath();
                if (!string.IsNullOrEmpty(sdkPackagePath))
                {
                    string packagePluginsPath = Path.Combine(sdkPackagePath, PATH_SEGMENT_RUNTIME, PATH_SEGMENT_PLUGINS, PATH_SEGMENT_IOS);
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
                    AddFrameworkSearchPath(proj, mainTarget, PATH_PACKAGES_PLUGINS_IOS);
                }
            }
            
            if (!config.ExcludeSetLDRunpathSearchPaths)
            {
                SetBuildPropertyIfDifferent(proj, mainTarget, BUILD_PROP_LD_RUNPATH_SEARCH_PATHS, BUILD_VALUE_EXECUTABLE_PATH_FRAMEWORKS);
            }
            
            if (!config.ExcludeSetSwiftVersion)
            {
                SetBuildPropertyIfDifferent(proj, mainTarget, BUILD_PROP_SWIFT_VERSION, BUILD_VALUE_SWIFT_VERSION);
            }
            
            if (!config.ExcludeSetSwiftStandardLibrariesForMain)
            {
                SetBuildPropertyIfDifferent(proj, mainTarget, BUILD_PROP_ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES, BUILD_VALUE_YES);
            }

            if (!config.ExcludeSetCodeSignEntitlements)
            {
                string entitlementPath = Path.Combine(pathToBuiltProject, ENTITLEMENTS_FILE_NAME);
                if (File.Exists(entitlementPath))
                {
                    SetBuildPropertyIfDifferent(proj, mainTarget, BUILD_PROP_CODE_SIGN_ENTITLEMENTS, ENTITLEMENTS_FILE_NAME);
                }
            }

            if (!config.ExcludeSetCodeSignStyle)
            {
                SetBuildPropertyIfDifferent(proj, mainTarget, BUILD_PROP_CODE_SIGN_STYLE, BUILD_VALUE_CODE_SIGN_STYLE_AUTOMATIC);
            }

            if (!config.ExcludeAddXCFramework)
            {
                AddXCFrameworkToProject(proj, pathToBuiltProject, mainTarget, XCFRAMEWORK_NAME);
            }

            proj.WriteToFile(projPath);
            string message = "[Appcharge PostBuild] Framework embedding and project processing complete.";
            _logger.Log(message);
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed to process Xcode project: {e.Message}";
            _logger.Log(errorMessage, true);
            Debug.LogWarning(errorMessage);
        }
    }

    private static void ProcessURLSchemes(string pathToBuiltProject, AppchargeConfig config)
    {
        string plistPath = Path.Combine(pathToBuiltProject, INFO_PLIST_FILE_NAME);
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

        bool identifierExists = false;
        foreach (var urlType in urlTypes.values)
        {
            var dict = urlType.AsDict();
            if (dict.values.ContainsKey(PLIST_KEY_CFBUNDLE_URL_NAME) && dict[PLIST_KEY_CFBUNDLE_URL_NAME].AsString() == URL_IDENTIFIER)
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
        
        if (!config.ExcludeSetURLSchemeTypeRole)
        {
            newURLType.SetString(PLIST_KEY_CFBUNDLE_TYPE_ROLE, PLIST_VALUE_TYPE_ROLE_EDITOR);
        }
        
        if (!config.ExcludeSetURLSchemeName)
        {
            newURLType.SetString(PLIST_KEY_CFBUNDLE_URL_NAME, URL_IDENTIFIER);
        }
        
        if (!config.ExcludeAddURLScheme)
        {
            PlistElementArray urlSchemes = newURLType.CreateArray(PLIST_KEY_CFBUNDLE_URL_SCHEMES);
            urlSchemes.AddString(URL_SCHEME_TEMPLATE);
        }

        plist.WriteToFile(plistPath);
        string successMessage = $"[Appcharge PostBuild] Successfully added URL scheme '{URL_SCHEME_TEMPLATE}' to Info.plist.";
        _logger.Log(successMessage);
        string finalContentAdded = plist.WriteToString();
        _logger.Log($"Final Info.plist content:\n{finalContentAdded}");
    }

    private static void AddFrameworkSearchPath(PBXProject proj, string target, string path)
    {
        string existingValue = proj.GetBuildPropertyForAnyConfig(target, BUILD_PROP_FRAMEWORK_SEARCH_PATHS);
        if (string.IsNullOrEmpty(existingValue))
        {
            proj.AddBuildProperty(target, BUILD_PROP_FRAMEWORK_SEARCH_PATHS, path);
            _logger.Log($"[Appcharge PostBuild] Added {BUILD_PROP_FRAMEWORK_SEARCH_PATHS} = {path} to target");
        }
        else if (!existingValue.Contains(path))
        {
            // Append the path if it doesn't already exist
            proj.AddBuildProperty(target, BUILD_PROP_FRAMEWORK_SEARCH_PATHS, path);
            _logger.Log($"[Appcharge PostBuild] Appended {BUILD_PROP_FRAMEWORK_SEARCH_PATHS} with {path}");
        }
    }

    private static string GetSDKPackagePath()
    {
        // Use Unity's PackageManager API to get the resolved path of the Appcharge SDK package
        try
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
                .FirstOrDefault(p => p.name == SDK_PACKAGE_NAME);
            
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                return packageInfo.resolvedPath;
            }
        }
        catch (System.Exception e)
        {
            _logger?.Log($"Failed to get SDK package path via PackageManager: {e.Message}", true);
            Debug.LogWarning($"[Appcharge PostBuild] Failed to get SDK package path via PackageManager: {e.Message}");
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
            _logger.Log($"[Appcharge PostBuild] Set {property} = {value} (was: {existingValue})");
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
                Path.Combine(PATH_SEGMENT_PACKAGES, SDK_PACKAGE_NAME, PATH_SEGMENT_RUNTIME, PATH_SEGMENT_PLUGINS, PATH_SEGMENT_IOS, xcframeworkName).Replace("\\", "/"),
                Path.Combine(PATH_SEGMENT_FRAMEWORKS, PATH_SEGMENT_PLUGINS, PATH_SEGMENT_IOS, xcframeworkName).Replace("\\", "/"),
                Path.Combine(PATH_SEGMENT_LIBRARIES, PATH_SEGMENT_PLUGINS, PATH_SEGMENT_IOS, xcframeworkName).Replace("\\", "/"),
                $"{PATH_SEGMENT_PACKAGES}/{SDK_PACKAGE_NAME}/{PATH_SEGMENT_RUNTIME}/{PATH_SEGMENT_PLUGINS}/{PATH_SEGMENT_IOS}/{xcframeworkName}",
                $"{PATH_SEGMENT_FRAMEWORKS}/{PATH_SEGMENT_PLUGINS}/{PATH_SEGMENT_IOS}/{xcframeworkName}",
                $"{PATH_SEGMENT_LIBRARIES}/{PATH_SEGMENT_PLUGINS}/{PATH_SEGMENT_IOS}/{xcframeworkName}",
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
                    
                    // Find file reference by name in comment: GUID /* ACPaymentLinks.xcframework */
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