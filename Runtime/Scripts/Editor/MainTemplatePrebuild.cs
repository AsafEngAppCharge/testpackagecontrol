using System;
using System.Collections.Generic;
using System.IO;
using Appcharge.AppDirect.Config;
using UnityEngine;

namespace Appcharge.AppDirect.Editor
{
    public sealed class MainTemplatePrebuild : Prebuilder
    {
        private const string CoreKtxDependency = "implementation 'androidx.core:core-ktx:1.15.0'";
        private const string CoreKtxIdentifier = "androidx.core:core-ktx";

        private const string AppCompatDependency = "implementation 'androidx.appcompat:appcompat:1.7.0'";
        private const string AppCompatIdentifier = "androidx.appcompat:appcompat";

        private const string KotlinSerializationJsonDependency = "implementation 'org.jetbrains.kotlinx:kotlinx-serialization-json:1.6.0'";
        private const string KotlinSerializationJsonIdentifier = "org.jetbrains.kotlinx:kotlinx-serialization-json";

        private const string KotlinCoroutinesCoreDependency = "implementation 'org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.3'";
        private const string KotlinCoroutinesCoreIdentifier = "org.jetbrains.kotlinx:kotlinx-coroutines-core";

        private const string KotlinCoroutinesAndroidDependency = "implementation 'org.jetbrains.kotlinx:kotlinx-coroutines-android:1.7.3'";
        private const string KotlinCoroutinesAndroidIdentifier = "org.jetbrains.kotlinx:kotlinx-coroutines-android";

        private const string WorkRuntimeKtxDependency = "implementation 'androidx.work:work-runtime-ktx:2.9.0'";
        private const string WorkRuntimeKtxIdentifier = "androidx.work:work-runtime-ktx";

        private const string LifecycleProcessDependency = "implementation 'androidx.lifecycle:lifecycle-process:2.8.7'";
        private const string LifecycleProcessIdentifier = "androidx.lifecycle:lifecycle-process";

        private const string MaterialDependency = "implementation 'com.google.android.material:material:1.12.0'";
        private const string MaterialIdentifier = "com.google.android.material:material";

        public MainTemplatePrebuild(string path, AppDirectPrebuildEditor prebuildEditor, AppDirectConfig config)
            : base(path, prebuildEditor, config)
        {
        }

        public override void Update()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    _prebuildEditor.LogToFile("mainTemplate.gradle file not found at path: " + _path, false);
                    return;
                }

                string original = File.ReadAllText(_path);
                string updated = MainTemplateGradle.Apply(
                    original,
                    EngineMetadataBuild.EngineName,
                    Application.unityVersion,
                    EngineMetadataBuild.EngineSdkVersion,
                    Dependencies(_config));

                if (updated != original)
                    File.WriteAllText(_path, updated);

                _prebuildEditor.LogToFile("Final mainTemplate.gradle content:\n" + updated);
            }
            catch (Exception ex)
            {
                _prebuildEditor.LogToFile("Error updating mainTemplate.gradle: " + ex.Message, true);
            }
        }

        public static List<(string dependency, string identifier)> Dependencies(AppDirectConfig config)
        {
            var dependencies = new List<(string, string)>();

            if (!config.ExcludeCoreKtx)
                dependencies.Add((CoreKtxDependency, CoreKtxIdentifier));
            if (!config.ExcludeAppCompat)
                dependencies.Add((AppCompatDependency, AppCompatIdentifier));
            if (!config.ExcludeKotlinSerializationJson)
                dependencies.Add((KotlinSerializationJsonDependency, KotlinSerializationJsonIdentifier));
            if (!config.ExcludeKotlinCoroutines)
            {
                dependencies.Add((KotlinCoroutinesCoreDependency, KotlinCoroutinesCoreIdentifier));
                dependencies.Add((KotlinCoroutinesAndroidDependency, KotlinCoroutinesAndroidIdentifier));
            }
            if (!config.ExcludeWorkRuntime)
                dependencies.Add((WorkRuntimeKtxDependency, WorkRuntimeKtxIdentifier));
            if (!config.ExcludeLifecycleProcess)
                dependencies.Add((LifecycleProcessDependency, LifecycleProcessIdentifier));
            if (!config.ExcludeMaterial)
                dependencies.Add((MaterialDependency, MaterialIdentifier));

            return dependencies;
        }
    }
}
