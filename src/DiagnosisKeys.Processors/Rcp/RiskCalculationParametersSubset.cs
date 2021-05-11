using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public class RiskCalculationParametersSubset
    {

        [JsonPropertyName("daysSinceOnsetToInfectiousness")]
        public InfectiousnessByDsosPair[] InfectiousnessByDsos { get; set; }
        public InfectiousnessByDsosPair[] InfectiousnessByTest { get; set; }
    }
}