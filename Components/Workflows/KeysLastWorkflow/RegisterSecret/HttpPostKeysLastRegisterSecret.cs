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

            await _Writer.Execute(args.Token);
            return new OkResult();
        }
    }
}