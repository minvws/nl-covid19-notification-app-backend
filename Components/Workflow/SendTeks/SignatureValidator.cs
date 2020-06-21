// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class SignatureValidator : ISignatureValidator
    {
        private readonly ILogger<SignatureValidator> _Logger;

        public SignatureValidator(ILogger<SignatureValidator> logger)
        {
            _Logger = logger;
        }

        public bool Valid(byte[] signature, KeyReleaseWorkflowState workflow, byte[] data)
        {
            if (signature == null)
                return false;

            using var hmac = new HMACSHA256(Convert.FromBase64String(workflow.ConfirmationKey));
            var hash = hmac.ComputeHash(data);

            var result = hash.SequenceEqual(signature);
            
            if (!result)
                _Logger.LogInformation($"Invalid signature for BucketId: {workflow.BucketId}");

            return result;
        }
    }
}