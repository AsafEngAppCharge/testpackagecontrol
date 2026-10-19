namespace Appcharge.AppDirect.Models
{
    public sealed class VersionData
    {
        public string PackageName { get; set; }
        public long VersionCode { get; set; }
        public string VersionName { get; set; }

        public override string ToString()
        {
            return PackageName + " " + VersionName + " (" + VersionCode + ")";
        }
    }
}
