using Google.Protobuf.Reflection;

namespace Iks.Protobuf
{
    public enum EfgsReportType {
        [OriginalName("UNKNOWN")] Unknown = 0,
        [OriginalName("CONFIRMED_TEST")] ConfirmedTest = 1,
        [OriginalName("CONFIRMED_CLINICAL_DIAGNOSIS")] ConfirmedClinicalDiagnosis = 2,
        [OriginalName("SELF_REPORT")] SelfReport = 3,
        [OriginalName("RECURSIVE")] Recursive = 4,
        [OriginalName("REVOKED")] Revoked = 5,
    }
}