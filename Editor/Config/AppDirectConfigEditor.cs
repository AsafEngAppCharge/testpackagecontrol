#if UNITY_EDITOR
using Appcharge.AppDirect.Config;
using UnityEditor;
using UnityEngine;

namespace Appcharge.AppDirect.PackageEditor
{
    [CustomEditor(typeof(AppDirectConfig))]
    public sealed class AppDirectConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Boot", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CheckoutPublicKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Environment"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableMainThreadDispatcher"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableDebugMode"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableIntegrationOptions"));

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeAndroidX"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeJetifier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeCoreKtx"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeAppCompat"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeKotlinSerializationJson"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeKotlinCoroutines"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeWorkRuntime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeLifecycleProcess"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExcludeMaterial"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnsureNativeManifestEntries"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
