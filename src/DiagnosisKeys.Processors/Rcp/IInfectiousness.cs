using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{ public interface IInfectiousness
    {
        public bool IsInfectious(InfectiousPeriodType infectiousPeriodType, int dsos);
    }
}
