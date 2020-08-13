using System;
using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class EksEngineResult
    {
        public DateTime Started { get; set; }
        public double SnapshotSeconds { get; set; }
        public double TotalSeconds { get; set; }
        public int InputCount { get; set; }
        public int TransmissionRiskNoneCount { get; set; }
        public int StuffingCount { get; set; }
        public int OutputCount { get; set; }
        public EksInfo[] EksInfo { get; set; }
        public int ReconcileOutputCount => InputCount + StuffingCount - TransmissionRiskNoneCount - OutputCount;
        public int ReconcileEksSumCount => EksInfo.Sum(x => x.TekCount) - OutputCount;

        //public int DupesFound { get; set; } ?
    }
}