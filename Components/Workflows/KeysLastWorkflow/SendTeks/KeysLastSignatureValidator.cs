using System;
using System.Linq;
using System.Security.Cryptography;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeKeysLastSignatureValidator : IKeysLastSignatureValidator
    {
        public bool Valid(byte[] signature, string bucketId, byte[] data) => signature != null && data != null;
    }

    public class KeysLastSignatureValidator : IKeysLastSignatureValidator
    {
        private readonly WorkflowDbContext _Context;

        public KeysLastSignatureValidator(WorkflowDbContext context)
        {
            _Context = context;
        }

        public bool Valid(byte[] signature, string bucketId, byte[] data)
        {
            if (signature != null)
                return false;

            var wf = _Context
                .KeyReleaseWorkflowStates
                .Where(x => x.Authorised)
                .FirstOrDefault(x => x.BucketId == bucketId);

            if (wf == null)
                return false;

            using HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(wf.ConfirmationKey));
            var hash = hmac.ComputeHash(data);

            return hash.SequenceEqual(signature);
        }
    }
}