namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public interface IJsonSerializer
    {
        string Serialize<TContent>(TContent input);
    }
}
