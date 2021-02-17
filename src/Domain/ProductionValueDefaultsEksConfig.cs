using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class ProductionValueDefaultsEksConfig : IEksConfig
    {
        public int TekCountMin => 150;
        public int TekCountMax => 150000;
        
        //There is no value set for this in deployment pipeline.
        public int PageSize => 1000;
        
        public bool CleanupDeletesData => false;
        public int LifetimeDays => 14;
    }
}