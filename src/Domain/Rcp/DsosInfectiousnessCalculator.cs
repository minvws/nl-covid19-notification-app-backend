using System;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public class DsosInfectiousnessCalculator : IDsosInfectiousnessCalculator
    {
        private readonly IRiskCalculationParametersReader _RiskCalculationParametersReader;

        public DsosInfectiousnessCalculator(IRiskCalculationParametersReader calculationParametersReader)
        {
            _RiskCalculationParametersReader = calculationParametersReader ?? throw new ArgumentNullException(nameof(calculationParametersReader));
        }

        public async Task<bool> IsInfectious(InfectiousPeriodType infectiousPeriodType, int dsos)
        {
            var days = _RiskCalculationParametersReader.GetInfectiousDaysAsync();
            return days[infectiousPeriodType].Contains(dsos);
        }
    }
}