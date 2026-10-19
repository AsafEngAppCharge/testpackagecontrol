namespace Appcharge.AppDirect.Models
{
    public sealed class UpdateInfo
    {
        public VersionData Current { get; set; }
        public VersionData New { get; set; }
        public UpdateState UpdateState { get; set; }
        public string Changelog { get; set; }
        public long? FileSizeBytes { get; set; }

        public bool HasUpdate => UpdateState != Appcharge.AppDirect.Models.UpdateState.None;

        public override string ToString()
        {
            return "state=" + UpdateState
                + " hasUpdate=" + HasUpdate
                + " current=[" + Current + "]"
                + " new=[" + New + "]"
                + " changelog=" + Changelog
                + " fileSizeBytes=" + FileSizeBytes;
        }
    }
}
