using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public interface IReader<out T> where T : ContentEntity
    {
        T? Execute(string id);
    }
}