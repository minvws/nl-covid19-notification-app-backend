using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public interface IReader<T> where T : ContentEntity
    {
        Task<T?> Execute(string id);
    }
}