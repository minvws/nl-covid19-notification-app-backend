// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.Authorisation
{
    public class HttpPostKeysFirstAuthorisationCommand
    {
        private readonly IKeysFirstAuthorisationTokenValidator _Validator;
        private readonly IKeysFirstAuthorisationWriter _Writer;
        public HttpPostKeysFirstAuthorisationCommand(IKeysFirstAuthorisationWriter writer, IKeysFirstAuthorisationTokenValidator validator)
        {
            _Writer = writer;
            _Validator = validator;
        }

        public IActionResult Execute(KeysFirstAuthorisationArgs args)
        {
            if (!ValidateAndCleanRequestMessage(args))
                return new BadRequestResult();

            _Writer.Execute(args);
            return new OkResult();
        }

        private bool ValidateAndCleanRequestMessage(KeysFirstAuthorisationArgs args)
        {
            if (args == null)
                return false;

            return _Validator.IsValid(args.Token);
        }
    }
}
