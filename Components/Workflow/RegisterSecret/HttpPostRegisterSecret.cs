// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class HttpPostRegisterSecret
    {
        private readonly ISecretWriter _Writer;
        private readonly ILogger _Logger;

        public HttpPostRegisterSecret(ISecretWriter writer, ILogger logger)
        {
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute()
        {
            try
            {
                var result = await _Writer.Execute();
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                _Logger.LogError(e.ToString());
                //TODO positive indication of an error to clients? Empty response? Or just 500?
                return new OkObjectResult(new EnrollmentResponse { Validity = -1 });
            }
        }
    }
}