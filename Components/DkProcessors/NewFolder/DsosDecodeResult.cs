using System.Collections.Generic;
using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public class DsosDecodeResult
    {
        public int Offset { get; set; }
        public SymptomStatus SymptomStatus  { get; set; }

        //TODO units? bananas?
        public int? IntervalDuration  { get; set; }
        public EncodedDsosType ValueType { get; set; }
        public int DecodedValue { get; set; }
        public DatePrecision DatePrecision { get; set; }
    }
}
