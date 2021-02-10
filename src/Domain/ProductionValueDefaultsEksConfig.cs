using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class ProductionValueDefaultsEksConfig : IEksConfig
    {
        public int TekCountMin => 150;
        public int TekCountMax => 150000;
        
        //There is no value set for this in deployment pipeline.
        public int PageSize => 10000;
        public bool CleanupDeletesData => throw new MissingConfigurationValueException(nameof(CleanupDeletesData));
        public int LifetimeDays => 14;
    }
}