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

        public async Task<bool> IsInfectious(int dsos)
        {
            var days = await _RiskCalculationParametersReader.GetInfectiousDaysAsync();
            return days.Contains(dsos);
        }
    }
}