using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Android
{
    public static class VersionDataConverter
    {
        public static VersionData ToVersionData(AndroidJavaObject javaObject)
        {
            if (javaObject == null)
                return null;

            return new VersionData
            {
                PackageName = javaObject.Call<string>("getPackageName"),
                VersionCode = javaObject.Call<long>("getVersionCode"),
                VersionName = javaObject.Call<string>("getVersionName")
            };
        }
    }
}
