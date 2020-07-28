namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TekWriteArgs
    {
        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
    }
}