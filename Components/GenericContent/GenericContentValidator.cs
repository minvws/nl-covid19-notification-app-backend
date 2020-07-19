using System.Text.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ContentLoading
{
    public class GenericContentValidator
    {
        public bool IsValid(GenericContentArgs args)
        {
            if (args == null)
                return false;

            if (!GenericContentTypes.IsValid(args.GenericContentType))
                return false;

            if (!IsValidJson(args.Json))
                return false;

            return true;
        }

        private bool IsValidJson(string json)
        {
            try
            {
                using var result = JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException ex)
            {
                //TODO log
                return false;
            }
        }
    }
}