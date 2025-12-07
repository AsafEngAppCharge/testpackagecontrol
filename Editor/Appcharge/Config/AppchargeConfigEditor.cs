#if UNITY_EDITOR
using Appcharge.PaymentLinks.Config;
using UnityEditor;

[CustomEditor(typeof(AppchargeConfig))]
public class AppchargeConfigEditor : Editor
{
    // Publisher Info
    SerializedProperty environment;
    SerializedProperty checkoutPublicKey;
    
    // Auto Integration
    SerializedProperty enableIntegrationOptions;
    SerializedProperty useInternalBrowser;
    
    // iOS Integration Settings
    SerializedProperty portraitOrientationLock;
    SerializedProperty addFrameworkToXcodeProject;
    SerializedProperty associatedDomain;
    SerializedProperty addURLScheme;
    
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

    // Debug Mode
    SerializedProperty enableDebugMode;

    void OnEnable()
    {
        // Publisher Info
        environment = serializedObject.FindProperty("Environment");
        checkoutPublicKey = serializedObject.FindProperty("CheckoutPublicKey");
        
        // Auto Integration
        enableIntegrationOptions = serializedObject.FindProperty("EnableIntegrationOptions");
        useInternalBrowser = serializedObject.FindProperty("UseInternalBrowser");
        portraitOrientationLock = serializedObject.FindProperty("PortraitOrientationLock");

        // iOS Integration Settings
        enableIntegrationOptions = serializedObject.FindProperty("EnableIntegrationOptions");
        addFrameworkToXcodeProject = serializedObject.FindProperty("AddFrameworkToXcodeProject");
        associatedDomain = serializedObject.FindProperty("AssociatedDomain");
        addURLScheme = serializedObject.FindProperty("AddURLScheme");
        
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
            EditorGUILayout.LabelField("Auto Integration Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("The following settings are automatically applied during the build process.\nIn order to manually configure the settings, you can disable each setting individually or turn off the auto integration option.", MessageType.Info);

            if (enableIntegrationOptions.boolValue)
            {
                if (isIOS)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(portraitOrientationLock);
                    EditorGUILayout.PropertyField(addFrameworkToXcodeProject);
                    EditorGUILayout.PropertyField(associatedDomain);
                    EditorGUILayout.PropertyField(addURLScheme);
                }

                if (isAndroid)
                {
                    EditorGUILayout.Space();
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

            if (enableIntegrationOptions.boolValue) {
                EditorGUILayout.PropertyField(useInternalBrowser);
            }

            EditorGUILayout.PropertyField(enableDebugMode);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif