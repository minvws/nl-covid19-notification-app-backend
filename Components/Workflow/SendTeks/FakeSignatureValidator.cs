using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeSignatureValidator : ISignatureValidator
    {
        public bool Valid(byte[] signature, string bucketId, byte[] data) => signature != null && data != null;
    }
}