using System.Text.Json.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public class InfectiousnessByDsosPair
    {
        [JsonPropertyName("daysSinceOnsetOfSymptoms")]
        public int Dsos { get; set; }

        [JsonPropertyName("infectiousness")]
        public int Value { get; set; }
    }
}