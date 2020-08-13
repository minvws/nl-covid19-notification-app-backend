using System;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public interface ISnapshotEksInput
    {
        Task<SnapshotEksInputResult> Execute(DateTime snapshotStart);
    }
}