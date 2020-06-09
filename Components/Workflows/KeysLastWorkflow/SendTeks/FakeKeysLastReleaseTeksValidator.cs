using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class FakeKeysLastReleaseTeksValidator : IKeysLastAuthorisationTokenValidator
    {
        public bool Valid(string token) => !string.IsNullOrWhiteSpace(token);
    }
}