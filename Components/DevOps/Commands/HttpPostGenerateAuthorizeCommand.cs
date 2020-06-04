// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Arguments;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Commands
{
    public class HttpPostGenerateAuthorizeCommand
    {
        private readonly GenerateAuthorizations _generator;

        public HttpPostGenerateAuthorizeCommand(GenerateAuthorizations generator)
        {
            _generator = generator;
        }

        public async Task<IActionResult> Execute(HttpPostGenerateAuthorizeArguments arguments)
        {
            try
            {
                if (0 >= arguments.pAuthorize || arguments.pAuthorize > 100)
                    throw new ArgumentException(nameof(arguments.pAuthorize));

                await _generator.Execute(arguments.pAuthorize, new Random(arguments.Seed));

                return new OkResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new BadRequestResult();
            }
        }
    }
}
