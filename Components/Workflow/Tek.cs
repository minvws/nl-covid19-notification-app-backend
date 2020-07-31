using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public class Tek
    {
        public byte[] KeyData { get; set; }

        public int RollingStartNumber { get; set; }
        //public DateTime RollingStartTime => RollingStartNumber.FromRollingPeriodStart();
        public DateTime StartDate => RollingStartNumber.FromRollingPeriodStart().Date;

        //Cannot be > 144 https://developer.apple.com/documentation/exposurenotification/setting_up_an_exposure_notification_server
        public int RollingPeriod { get; set; }

        public int End => RollingStartNumber + RollingPeriod;

        public string Region { get; set; } = DefaultValues.Region;

        public PublishingState PublishingState { get; set; } = PublishingState.Unpublished;
        public DateTime PublishAfter { get; set; }
        public bool SameTime(Tek other)
        {
            return RollingStartNumber == other.RollingStartNumber && RollingPeriod == other.RollingPeriod;
        }
    }
}