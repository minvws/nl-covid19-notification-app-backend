using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public interface IRiskCalculationParametersReader
    { 
        Dictionary<InfectiousPeriodType, HashSet<int>> GetInfectiousDaysAsync();
    }
}