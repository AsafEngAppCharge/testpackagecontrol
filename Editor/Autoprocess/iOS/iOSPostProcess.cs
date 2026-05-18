#if UNITY_IOS
using Appcharge.PaymentLinks.Config;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Appcharge.PaymentLinks.Editor;

public static class iOSPostProcess
{
    private static PrebuildLogger _logger;

    // File names
    private const string ENTITLEMENTS_FILE_NAME = "Appcharge.entitlements";
    private const string INFO_PLIST_FILE_NAME = "Info.plist";
    private const string LEGACY_XCFRAMEWORK_NAME = "ACPaymentLinks.xcframework";

    // Build property names
    private const string BUILD_PROP_ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
    private const string BUILD_PROP_LD_RUNPATH_SEARCH_PATHS = "LD_RUNPATH_SEARCH_PATHS";
    private const string BUILD_PROP_SWIFT_VERSION = "SWIFT_VERSION";
    private const string BUILD_PROP_CODE_SIGN_ENTITLEMENTS = "CODE_SIGN_ENTITLEMENTS";
    private const string BUILD_PROP_CODE_SIGN_STYLE = "CODE_SIGN_STYLE";

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
    
    // Package name
    private const string SDK_PACKAGE_NAME = "com.appcharge.paymentlinks";
    
    // Config path
    private const string CONFIG_ASSET_PATH = "Assets/Resources/Appcharge/AppchargeConfig.asset";

    private const string SPM_PACKAGE_URL = "https://github.com/Appcharge/ios-payment-links.git";
    private const string SPM_PACKAGE_VERSION = "1.7.0";
    private const string SPM_PRODUCT_NAME = "ACPaymentLinks";

    private const string SPM_PACKAGE_LOCAL_PATH = "/Users/asafenglander/git/ios-payment-links";
    private const string SPM_PACKAGE_LOCAL_BRANCH = "local";

    [PostProcessBuild(999)]
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
            ProcessSwiftPackage(pathToBuiltProject, config);
        
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

    private static void ProcessSwiftPackage(string pathToBuiltProject, AppchargeConfig config)
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

            if (!config.ExcludeSetSwiftStandardLibrariesForFramework)
            {
                SetBuildPropertyIfDifferent(proj, unityFrameworkTarget, BUILD_PROP_ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES, BUILD_VALUE_NO);
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
                RemoveLegacyXCFrameworkFromTarget(proj, pathToBuiltProject, mainTarget, LEGACY_XCFRAMEWORK_NAME);
                RemoveLegacyXCFrameworkFromTarget(proj, pathToBuiltProject, unityFrameworkTarget, LEGACY_XCFRAMEWORK_NAME);

                ResolveSpmPackageReference(out string spmUrl, out bool spmUseBranch, out string spmBranchOrVersion);
                AddSwiftPackageToProject(proj, projPath, mainTarget, spmUrl, spmUseBranch, spmBranchOrVersion, SPM_PRODUCT_NAME);
                AddSwiftPackageToProject(proj, projPath, unityFrameworkTarget, spmUrl, spmUseBranch, spmBranchOrVersion, SPM_PRODUCT_NAME);
            }
            else
            {
                _logger.Log("[Appcharge PostBuild] ExcludeAddXCFramework is enabled: skipping remote Swift package link and legacy xcframework removal.");
            }

            proj.WriteToFile(projPath);
            _logger.Log("[Appcharge PostBuild] SPM integration complete.");
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed to process SPM integration: {e.Message}";
            _logger.Log(errorMessage, true);
            Debug.LogWarning(errorMessage);
        }
    }

    // Remote: exact SPM_PACKAGE_VERSION on SPM_PACKAGE_URL. Local: file URL + SPM_PACKAGE_LOCAL_BRANCH (git branch, no semver tag required).
    private static void ResolveSpmPackageReference(out string packageUrl, out bool useBranch, out string branchOrExactVersion)
    {
        useBranch = false;
        branchOrExactVersion = SPM_PACKAGE_VERSION;
        packageUrl = SPM_PACKAGE_URL;

        if (string.IsNullOrWhiteSpace(SPM_PACKAGE_LOCAL_PATH))
            return;

        string fullPath = Path.GetFullPath(SPM_PACKAGE_LOCAL_PATH.Trim());
        if (!Directory.Exists(fullPath))
        {
            _logger.Log($"[Appcharge PostBuild] SPM_PACKAGE_LOCAL_PATH is not a directory: '{fullPath}'. Using remote SPM URL instead.", true);
            return;
        }

        try
        {
            packageUrl = new Uri(fullPath).AbsoluteUri;
            useBranch = true;
            branchOrExactVersion = SPM_PACKAGE_LOCAL_BRANCH;
            _logger.Log($"[Appcharge PostBuild] Using local Swift package: {packageUrl} (branch: {branchOrExactVersion})");
        }
        catch (System.Exception e)
        {
            _logger.Log($"[Appcharge PostBuild] Invalid SPM_PACKAGE_LOCAL_PATH: {e.Message}. Using remote SPM URL.", true);
            packageUrl = SPM_PACKAGE_URL;
            useBranch = false;
            branchOrExactVersion = SPM_PACKAGE_VERSION;
        }
    }

    private static void AddSwiftPackageToProject(
        PBXProject proj,
        string projPath,
        string targetGuid,
        string packageUrl,
        bool useBranch,
        string branchOrExactVersion,
        string productName)
    {
        try
        {
            string pbxContentBefore = File.Exists(projPath) ? File.ReadAllText(projPath) : string.Empty;
            string packageGuid = FindExistingPackageGuid(pbxContentBefore, packageUrl);

            if (string.IsNullOrEmpty(packageGuid))
            {
                if (useBranch)
                {
                    packageGuid = proj.AddRemotePackageReferenceAtBranch(packageUrl, branchOrExactVersion);
                    _logger.Log($"[Appcharge PostBuild] Added Swift package reference (branch): {packageUrl} @ {branchOrExactVersion} (GUID: {packageGuid})");
                }
                else
                {
                    packageGuid = proj.AddRemotePackageReferenceAtVersion(packageUrl, branchOrExactVersion);
                    _logger.Log($"[Appcharge PostBuild] Added Swift package reference (exact version): {packageUrl} @ {branchOrExactVersion} (GUID: {packageGuid})");
                }
            }
            else
            {
                _logger.Log($"[Appcharge PostBuild] Reusing existing Swift package reference: {packageUrl} (GUID: {packageGuid})");
            }

            proj.AddRemotePackageFrameworkToProject(targetGuid, productName, packageGuid, false);
            _logger.Log($"[Appcharge PostBuild] Linked Swift package product '{productName}' to target.");
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed to add Swift package '{productName}': {e.Message}";
            _logger.Log(errorMessage, true);
            Debug.LogWarning(errorMessage);
        }
    }

    private static string FindExistingPackageGuid(string pbxContent, string packageUrl)
    {
        if (string.IsNullOrEmpty(pbxContent) || string.IsNullOrEmpty(packageUrl))
            return null;

        string escapedUrl = Regex.Escape(packageUrl);
        Match match = Regex.Match(
            pbxContent,
            $@"([A-F0-9]{{24}})\s*/\*.*\*/\s*=\s*\{{[^}}]*repositoryURL\s*=\s*""{escapedUrl}"";",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );

        return match.Success ? match.Groups[1].Value : null;
    }

    private static string FindFileGuidForXCFramework(PBXProject proj, string pathToBuiltProject, string xcframeworkName)
    {
        string[] possiblePaths = new string[]
        {
            $"Packages/{SDK_PACKAGE_NAME}/Runtime/Plugins/iOS/{xcframeworkName}",
            $"Frameworks/Plugins/iOS/{xcframeworkName}",
            $"Libraries/Plugins/iOS/{xcframeworkName}",
            xcframeworkName
        };

        foreach (string possiblePath in possiblePaths)
        {
            string guid = proj.FindFileGuidByProjectPath(possiblePath);
            if (!string.IsNullOrEmpty(guid))
            {
                _logger.Log($"[Appcharge PostBuild] Found legacy xcframework at path: {possiblePath}, GUID: {guid}");
                return guid;
            }
        }

        string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        if (File.Exists(pbxPath))
        {
            string pbxContent = File.ReadAllText(pbxPath);
            Match match = Regex.Match(
                pbxContent,
                $@"([A-F0-9]{{24}})\s*/\*\s*{Regex.Escape(xcframeworkName)}\s*\*/\s*=\s*{{isa\s*=\s*PBXFileReference;",
                RegexOptions.IgnoreCase
            );

            if (match.Success)
            {
                string guid = match.Groups[1].Value;
                _logger.Log($"[Appcharge PostBuild] Found legacy xcframework by PBX comment, GUID: {guid}");
                return guid;
            }
        }

        return null;
    }

    private static void RemoveLegacyXCFrameworkFromTarget(
        PBXProject proj,
        string pathToBuiltProject,
        string targetGuid,
        string xcframeworkName)
    {
        try
        {
            string fileGuid = FindFileGuidForXCFramework(proj, pathToBuiltProject, xcframeworkName);
            if (string.IsNullOrEmpty(fileGuid))
            {
                _logger.Log($"[Appcharge PostBuild] Legacy xcframework '{xcframeworkName}' not found in project.");
                return;
            }

            proj.RemoveFileFromBuild(targetGuid, fileGuid);
            _logger.Log($"[Appcharge PostBuild] Removed legacy xcframework '{xcframeworkName}' from target build phase.");
        }
        catch (System.Exception e)
        {
            string errorMessage = $"[Appcharge PostBuild] Failed removing legacy xcframework '{xcframeworkName}' from target: {e.Message}";
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

    private static void SetBuildPropertyIfDifferent(PBXProject proj, string target, string property, string value)
    {
        string existingValue = proj.GetBuildPropertyForAnyConfig(target, property);
        if (existingValue != value)
        {
            proj.SetBuildProperty(target, property, value);
            _logger.Log($"[Appcharge PostBuild] Set {property} = {value} (was: {existingValue})");
        }
    }
}
#endif