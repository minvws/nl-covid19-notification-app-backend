namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public interface IIksCleaningConfig
    {
        int LifetimeDays { get; }
    }
}
