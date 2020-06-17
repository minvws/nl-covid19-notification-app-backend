// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class SignatureValidator : ISignatureValidator
    {
        private readonly WorkflowDbContext _Context;

        public SignatureValidator(WorkflowDbContext context)
        {
            _Context = context;
        }

        public bool Valid(byte[] signature, string bucketId, byte[] data)
        {
            if (signature == null)
                return false;

            var wf = _Context
                .KeyReleaseWorkflowStates
                .Where(x => x.Authorised)
                .FirstOrDefault(x => x.BucketId == bucketId);

            if (wf == null)
                return false;

            using var hmac = new HMACSHA256(Convert.FromBase64String(wf.ConfirmationKey));
            var hash = hmac.ComputeHash(data);

            return hash.SequenceEqual(signature);
        }
    }
}