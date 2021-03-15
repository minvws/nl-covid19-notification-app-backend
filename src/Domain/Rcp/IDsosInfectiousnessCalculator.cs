using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    public interface IDsosInfectiousnessCalculator
    {
        Task<bool> IsInfectious(int dsos);
    }
}