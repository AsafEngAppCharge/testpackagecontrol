using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Appcharge.AppDirect.Editor
{
    public static class MainTemplateGradle
    {
        private const string EngineNamePlaceholder = "ENGINE_NAME";
        private const string EngineVersionNamePlaceholder = "ENGINE_VERSION_NAME";
        private const string EngineSdkVersionPlaceholder = "ENGINE_SDK_VERSION";

        public static string Apply(
            string gradle,
            string engineName,
            string unityVersion,
            string sdkVersion,
            IReadOnlyList<(string dependency, string identifier)> dependencies)
        {
            string gradleTemplate = RemoveEngineBuildConfigFields(gradle ?? string.Empty);
            var missing = new List<string>();
            foreach (var (dependency, identifier) in dependencies)
            {
                if (ReplaceDependency(ref gradleTemplate, dependency, identifier))
                    continue;
                missing.Add(dependency);
            }

            const string defaultConfigMarker = "defaultConfig {";
            if (gradleTemplate.Contains(defaultConfigMarker))
            {
                string name = GroovyDoubleQuotedString(engineName);
                string version = GroovyDoubleQuotedString(unityVersion);
                string sdk = GroovyDoubleQuotedString(sdkVersion);
                gradleTemplate = UpsertManifestPlaceholder(gradleTemplate, EngineNamePlaceholder, name, out bool insertName);
                gradleTemplate = UpsertManifestPlaceholder(gradleTemplate, EngineVersionNamePlaceholder, version, out bool insertVersion);
                gradleTemplate = UpsertManifestPlaceholder(gradleTemplate, EngineSdkVersionPlaceholder, sdk, out bool insertSdk);

                string injection = string.Empty;
                if (insertName)
                    injection += "\n        manifestPlaceholders[\"" + EngineNamePlaceholder + "\"] = " + name;
                if (insertVersion)
                    injection += "\n        manifestPlaceholders[\"" + EngineVersionNamePlaceholder + "\"] = " + version;
                if (insertSdk)
                    injection += "\n        manifestPlaceholders[\"" + EngineSdkVersionPlaceholder + "\"] = " + sdk;

                if (injection.Length > 0)
                {
                    int insertAfterOpen = gradleTemplate.IndexOf(defaultConfigMarker) + defaultConfigMarker.Length;
                    gradleTemplate = gradleTemplate.Insert(insertAfterOpen, injection);
                }
            }

            if (missing.Count > 0)
                gradleTemplate = InsertMissingDependencies(gradleTemplate, missing);

            return gradleTemplate;
        }

        public static string GroovyDoubleQuotedString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\"";
        }

        private static string UpsertManifestPlaceholder(string gradleTemplate, string key, string quotedValue, out bool needsInsert)
        {
            needsInsert = false;
            string assignmentPrefix = "manifestPlaceholders[\"" + key + "\"]";
            string pattern = "manifestPlaceholders\\[\"" + Regex.Escape(key) + "\"\\]\\s*=\\s*\"[^\"]*\"";
            var regex = new Regex(pattern);
            if (regex.IsMatch(gradleTemplate))
                return regex.Replace(gradleTemplate, assignmentPrefix + " = " + quotedValue, 1);

            if (gradleTemplate.Contains(assignmentPrefix))
                return gradleTemplate;

            needsInsert = true;
            return gradleTemplate;
        }

        private static string RemoveEngineBuildConfigFields(string gradleTemplate)
        {
            return Regex.Replace(
                gradleTemplate,
                @"^\s*(?://\s*)?buildConfigField\s+""String"",\s*""ENGINE_(?:NAME|VERSION_NAME|SDK_VERSION)""[^\n]*\r?\n",
                string.Empty,
                RegexOptions.Multiline);
        }

        private static bool ReplaceDependency(ref string gradleTemplate, string dependency, string identifier)
        {
            var regex = new Regex("implementation\\s+'" + Regex.Escape(identifier) + ":[^']*'");
            if (!regex.IsMatch(gradleTemplate))
                return false;

            gradleTemplate = regex.Replace(gradleTemplate, dependency);
            return true;
        }

        private static string InsertMissingDependencies(string gradleTemplate, List<string> missingDependencies)
        {
            int insertIndex = -1;
            int depsMarkerIndex = gradleTemplate.IndexOf("**DEPS**");
            if (depsMarkerIndex >= 0)
            {
                insertIndex = depsMarkerIndex + "**DEPS**".Length;
            }
            else
            {
                int depsBlockIndex = gradleTemplate.LastIndexOf("dependencies {");
                if (depsBlockIndex >= 0)
                    insertIndex = FindBlockEnd(gradleTemplate, depsBlockIndex);
            }

            if (insertIndex < 0)
                return gradleTemplate;

            string dependenciesToInsert = "\n" + string.Join("\n", missingDependencies) + "\n";
            return gradleTemplate.Insert(insertIndex, dependenciesToInsert);
        }

        private static int FindBlockEnd(string text, int blockStart)
        {
            int braceCount = 0;
            for (int i = blockStart; i < text.Length; i++)
            {
                if (text[i] == '{')
                    braceCount++;
                else if (text[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0)
                        return i;
                }
            }

            return -1;
        }
    }
}
