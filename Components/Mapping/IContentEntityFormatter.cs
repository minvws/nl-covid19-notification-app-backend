using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public interface IContentEntityFormatter
    {
        Task<TContentEntity> Fill<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity;
    }
}