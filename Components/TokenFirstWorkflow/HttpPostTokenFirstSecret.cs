// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class HttpPostTokenFirstSecret
    {
        private readonly ITokenFirstSecretValidator _Validator;
        private readonly ITokenFirstSecretWriter _Writer;

        public HttpPostTokenFirstSecret(ITokenFirstSecretValidator validator, ITokenFirstSecretWriter writer)
        {
            _Validator = validator;
            _Writer = writer;
        }

        public IActionResult Execute(TokenFirstSecretArgs args)
        {
            if (!_Validator.Valid(args))
            {
                //TODO log bad token
                return new OkResult();
            }

            _Writer.Execute(args.Token);
            return new OkResult();
        }
    }
}