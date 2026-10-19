using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Android
{
    public static class UpdateInfoConverter
    {
        public static UpdateInfo ToUpdateInfo(AndroidJavaObject javaObject)
        {
            if (javaObject == null)
                return null;

            return new UpdateInfo
            {
                Current = VersionDataConverter.ToVersionData(javaObject.Call<AndroidJavaObject>("getCurrent")),
                New = VersionDataConverter.ToVersionData(javaObject.Call<AndroidJavaObject>("getNew")),
                UpdateState = (UpdateState)javaObject.Call<int>("getUpdateState"),
                Changelog = javaObject.Call<string>("getChangelog"),
                FileSizeBytes = ReadNullableLong(javaObject.Call<AndroidJavaObject>("getFileSizeBytes"))
            };
        }

        private static long? ReadNullableLong(AndroidJavaObject boxed)
        {
            if (boxed == null)
                return null;

            return boxed.Call<long>("longValue");
        }
    }
}
