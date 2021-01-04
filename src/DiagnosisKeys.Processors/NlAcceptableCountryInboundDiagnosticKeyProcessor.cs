using System.Collections.Generic;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class NlAcceptableCountryInboundDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        private readonly HashSet<string> _Include;

        public NlAcceptableCountryInboundDiagnosticKeyProcessor(IAcceptableCountriesSetting settings)
        {
            _Include = settings.AcceptableCountries.ToHashSet();
        }

        //Assumes a cleaning processor has already done Trim/ToUpper
        public DkProcessingItem? Execute(DkProcessingItem? value)
            => value.Metadata.TryGetValue("Source", out var source) && _Include.Contains(source) ? value : null;
    }
}