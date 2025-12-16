#if UNITY_EDITOR
using Appcharge.PaymentLinks.Config;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AppchargeConfig))]
public class AppchargeConfigEditor : Editor
{
    // Publisher Info
    SerializedProperty environment;
    SerializedProperty checkoutPublicKey;
    
    // Auto Integration
    SerializedProperty enableIntegrationOptions;
    
    // iOS Integration Settings
    SerializedProperty portraitOrientationLock;
    SerializedProperty enableIOSEntitlementsIntegration;
    SerializedProperty enableIOSURLSchemeIntegration;
    SerializedProperty enableIOSFrameworkIntegration;
    SerializedProperty associatedDomain;
    SerializedProperty iOSBrowserMode;
    
    // iOS Entitlements Integration Exclusions
    SerializedProperty excludeCreateEntitlementsFile;
    SerializedProperty excludeCreateAssociatedDomainsKey;
    SerializedProperty excludeAddAssociatedDomain;
    
    // iOS URL Scheme Integration Exclusions
    SerializedProperty excludeSetURLSchemeTypeRole;
    SerializedProperty excludeSetURLSchemeName;
    SerializedProperty excludeAddURLScheme;
    
    // iOS Framework Integration Exclusions
    SerializedProperty excludeSetSwiftStandardLibrariesForFramework;
    SerializedProperty excludeAddFrameworkSearchPaths;
    SerializedProperty excludeSetLDRunpathSearchPaths;
    SerializedProperty excludeSetSwiftVersion;
    SerializedProperty excludeSetSwiftStandardLibrariesForMain;
    SerializedProperty excludeSetCodeSignEntitlements;
    SerializedProperty excludeSetCodeSignStyle;
    SerializedProperty excludeAddXCFramework;
    
    // Android Integration Settings
    SerializedProperty excludeAndroidX;
    SerializedProperty excludeJetifier;
    SerializedProperty excludeAppcompat;
    SerializedProperty excludeAndroidbrowser;
    SerializedProperty excludeKotlin;
    SerializedProperty excludeInternetPermission;
    SerializedProperty excludeQueriesBlock;
    SerializedProperty excludeAppchargeActivity;
    SerializedProperty excludeAppchargeActivityIntentFilters;
    SerializedProperty excludeExportedAttribute;
    SerializedProperty excludeCustomScheme;
    SerializedProperty excludeCustomHost;
    SerializedProperty excludeHttpsSchemeInActivity;
    SerializedProperty excludeDiscouragedApiTool;
    SerializedProperty AndroidBrowserMode;
    // Debug Mode
    SerializedProperty enableDebugMode;

    void OnEnable()
    {
        // Publisher Info
        environment = serializedObject.FindProperty("Environment");
        checkoutPublicKey = serializedObject.FindProperty("CheckoutPublicKey");
        
        // Auto Integration
        enableIntegrationOptions = serializedObject.FindProperty("EnableIntegrationOptions");
        iOSBrowserMode = serializedObject.FindProperty("iOSBrowserMode");
        AndroidBrowserMode = serializedObject.FindProperty("AndroidBrowserMode");
        portraitOrientationLock = serializedObject.FindProperty("PortraitOrientationLock");

        // iOS Integration Settings
        enableIntegrationOptions = serializedObject.FindProperty("EnableIntegrationOptions");
        enableIOSEntitlementsIntegration = serializedObject.FindProperty("EnableIOSEntitlementsIntegration");
        enableIOSURLSchemeIntegration = serializedObject.FindProperty("EnableIOSURLSchemeIntegration");
        enableIOSFrameworkIntegration = serializedObject.FindProperty("EnableIOSFrameworkIntegration");
        associatedDomain = serializedObject.FindProperty("AssociatedDomain");
        
        // iOS Entitlements Integration Exclusions
        excludeCreateEntitlementsFile = serializedObject.FindProperty("ExcludeCreateEntitlementsFile");
        excludeCreateAssociatedDomainsKey = serializedObject.FindProperty("ExcludeCreateAssociatedDomainsKey");
        excludeAddAssociatedDomain = serializedObject.FindProperty("ExcludeAddAssociatedDomain");
        
        // iOS URL Scheme Integration Exclusions
        excludeSetURLSchemeTypeRole = serializedObject.FindProperty("ExcludeSetURLSchemeTypeRole");
        excludeSetURLSchemeName = serializedObject.FindProperty("ExcludeSetURLSchemeName");
        excludeAddURLScheme = serializedObject.FindProperty("ExcludeAddURLScheme");
        
        // iOS Framework Integration Exclusions
        excludeSetSwiftStandardLibrariesForFramework = serializedObject.FindProperty("ExcludeSetSwiftStandardLibrariesForFramework");
        excludeAddFrameworkSearchPaths = serializedObject.FindProperty("ExcludeAddFrameworkSearchPaths");
        excludeSetLDRunpathSearchPaths = serializedObject.FindProperty("ExcludeSetLDRunpathSearchPaths");
        excludeSetSwiftVersion = serializedObject.FindProperty("ExcludeSetSwiftVersion");
        excludeSetSwiftStandardLibrariesForMain = serializedObject.FindProperty("ExcludeSetSwiftStandardLibrariesForMain");
        excludeSetCodeSignEntitlements = serializedObject.FindProperty("ExcludeSetCodeSignEntitlements");
        excludeSetCodeSignStyle = serializedObject.FindProperty("ExcludeSetCodeSignStyle");
        excludeAddXCFramework = serializedObject.FindProperty("ExcludeAddXCFramework");
        
        // Android Integration Settings
        excludeAndroidX = serializedObject.FindProperty("ExcludeAndroidX");
        excludeJetifier = serializedObject.FindProperty("ExcludeJetifier");
        excludeAppcompat = serializedObject.FindProperty("ExcludeAppcompat");
        excludeAndroidbrowser = serializedObject.FindProperty("ExcludeAndroidbrowser");
        excludeKotlin = serializedObject.FindProperty("ExcludeKotlin");
        excludeInternetPermission = serializedObject.FindProperty("ExcludeInternetPermission");
        excludeQueriesBlock = serializedObject.FindProperty("ExcludeQueriesBlock");
        excludeAppchargeActivity = serializedObject.FindProperty("ExcludeAppchargeActivity");
        excludeAppchargeActivityIntentFilters = serializedObject.FindProperty("ExcludeAppchargeActivityIntentFilters");
        excludeExportedAttribute = serializedObject.FindProperty("ExcludeExportedAttribute");
        excludeCustomScheme = serializedObject.FindProperty("ExcludeCustomScheme");
        excludeCustomHost = serializedObject.FindProperty("ExcludeCustomHost");
        excludeHttpsSchemeInActivity = serializedObject.FindProperty("ExcludeHttpsSchemeInActivity");
        excludeDiscouragedApiTool = serializedObject.FindProperty("ExcludeDiscouragedApiTool");
        
        // Debug Mode
        enableDebugMode = serializedObject.FindProperty("EnableDebugMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Publisher Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(environment);
        EditorGUILayout.PropertyField(checkoutPublicKey);
        EditorGUILayout.Space();
       
        bool isIOS = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
        bool isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;

        if (isAndroid || isIOS) {
            if (enableIntegrationOptions.boolValue)
            {
                EditorGUILayout.LabelField("Platform Auto Integration Settings", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The following settings are automatically applied during the build process.\nIn order to manually configure the settings, you can disable each setting individually or turn off the auto integration option.", MessageType.Info);
                
                if (isIOS)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(iOSBrowserMode);
                    EditorGUILayout.PropertyField(associatedDomain);
                    EditorGUILayout.PropertyField(portraitOrientationLock);
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(enableIOSFrameworkIntegration);
                    if (enableIOSFrameworkIntegration.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(excludeSetSwiftStandardLibrariesForFramework);
                        EditorGUILayout.PropertyField(excludeAddFrameworkSearchPaths);
                        EditorGUILayout.PropertyField(excludeSetLDRunpathSearchPaths);
                        EditorGUILayout.PropertyField(excludeSetSwiftVersion);
                        EditorGUILayout.PropertyField(excludeSetSwiftStandardLibrariesForMain);
                        EditorGUILayout.PropertyField(excludeSetCodeSignEntitlements);
                        EditorGUILayout.PropertyField(excludeSetCodeSignStyle);
                        EditorGUILayout.PropertyField(excludeAddXCFramework);
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(enableIOSEntitlementsIntegration);
                    if (enableIOSEntitlementsIntegration.boolValue && associatedDomain.stringValue != "")
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(excludeCreateEntitlementsFile);
                        EditorGUILayout.PropertyField(excludeCreateAssociatedDomainsKey);
                        EditorGUILayout.PropertyField(excludeAddAssociatedDomain);
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(enableIOSURLSchemeIntegration, new GUIContent("Enable iOS URL Scheme Integration"));
                    if (enableIOSURLSchemeIntegration.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(excludeSetURLSchemeTypeRole);
                        EditorGUILayout.PropertyField(excludeSetURLSchemeName);
                        EditorGUILayout.PropertyField(excludeAddURLScheme);
                        EditorGUI.indentLevel--;
                    }
                }

                if (isAndroid)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(AndroidBrowserMode);
                    EditorGUILayout.PropertyField(excludeAndroidX);
                    EditorGUILayout.PropertyField(excludeJetifier);
                    EditorGUILayout.PropertyField(excludeAppcompat);
                    EditorGUILayout.PropertyField(excludeAndroidbrowser);
                    EditorGUILayout.PropertyField(excludeKotlin);

                    EditorGUILayout.PropertyField(excludeInternetPermission);
                    EditorGUILayout.PropertyField(excludeQueriesBlock);
                    EditorGUILayout.PropertyField(excludeAppchargeActivity);
                    
                    if (!excludeAppchargeActivity.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(excludeExportedAttribute);
                        EditorGUILayout.PropertyField(excludeDiscouragedApiTool);
                        EditorGUILayout.PropertyField(excludeAppchargeActivityIntentFilters);
                        EditorGUILayout.PropertyField(excludeCustomScheme);
                        EditorGUILayout.PropertyField(excludeCustomHost);
                        EditorGUILayout.PropertyField(excludeHttpsSchemeInActivity);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(enableIntegrationOptions);
            EditorGUILayout.PropertyField(enableDebugMode);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif