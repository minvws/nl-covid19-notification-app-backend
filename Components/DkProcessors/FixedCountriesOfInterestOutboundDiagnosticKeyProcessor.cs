using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public class FixedCountriesOfInterestOutboundDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly string[] _Value;

        public FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(IFixedCountriesOfInterestSetting settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _Value = settings.CountriesOfInterest;
        }

        //Assumes a cleaning processor has already done Trim/ToUpper
        public DkProcessingItem? Execute(DkProcessingItem? value)
        { 
            value.Metadata.Remove("CountriesOfInterest");
            value.Metadata.Add("CountriesOfInterest", _Value);
            return value;
        }
    }
}