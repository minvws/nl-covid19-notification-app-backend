// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class HttpPostKeysFirstRandomAuthorisationCommand
    {
        private readonly GenerateKeysFirstAuthorisations _Generator;

        public HttpPostKeysFirstRandomAuthorisationCommand(GenerateKeysFirstAuthorisations generator)
        {
            _Generator = generator;
        }

        public async Task<IActionResult> Execute(KeysFirstRandomAuthorisationArgs arguments)
        {
                if (0 >= arguments.pAuthorize || arguments.pAuthorize > 100)
                    throw new ArgumentException(nameof(arguments.pAuthorize));

                await _Generator.Execute(arguments.pAuthorize, new Random(arguments.Seed));

                return new OkResult();
        }
    }
}
