using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public interface IMarkWorkFlowTeksAsUsed
    {
        Task<MarkWorkFlowTeksAsUsedResult> ExecuteAsync();
    }
}