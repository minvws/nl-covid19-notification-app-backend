using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public interface IJsonSerializer
    {
        string Serialize<TContent>(TContent input);
        TContent Deserialize<TContent>(string input);
    }

}
