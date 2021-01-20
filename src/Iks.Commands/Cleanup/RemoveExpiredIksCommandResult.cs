namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class RemoveExpiredIksCommandResult
    {
        public int GivenMercy { get; set; }
        public int Found { get; set; }
        public int Zombies { get; set; }
        public int Remaining { get; set; }

        public int Reconciliation => Found - GivenMercy - Remaining;
    }
}
