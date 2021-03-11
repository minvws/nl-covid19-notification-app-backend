using System.Collections.Generic;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public interface IRiskCalculationParametersReader
    {
        Task<HashSet<int>> GetInfectiousDaysAsync();
    }
}