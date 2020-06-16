// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class HttpPostKeysLastRegisterSecret
    {
        private readonly IKeysLastSecretValidator _Validator;
        private readonly IKeysLastSecretWriter _Writer;

        public HttpPostKeysLastRegisterSecret(IKeysLastSecretValidator validator, IKeysLastSecretWriter writer)
        {
            _Validator = validator;
            _Writer = writer;
        }

        public async Task<IActionResult> Execute(KeysLastSecretArgs args)
        {
            if (!_Validator.Valid(args))
            {
                //TODO log bad token
                return new OkResult();
            }

            await _Writer.Execute(args.ConfirmationKey);
            return new OkObjectResult(new EnrollmentResponse());
        }
    }

    public class EnrollmentResponse
    {
        //description: Confirmation code that the phone must display so the user can read it to an operator.
        public string LabConfirmationId { get; set; }
        //description: Random generated number that will later in the upload process associate the keys with the correct signature
        public string BucketId { get; set; }
    }
}