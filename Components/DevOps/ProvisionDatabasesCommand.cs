// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ProvisionDatabasesCommand
    {
        private readonly IConfiguration _Configuration;
        private readonly IContentSigner _ContentSigner;
        private readonly IJsonSerializer _JsonSerializer;

        public ProvisionDatabasesCommand(IConfiguration configuration, IContentSigner contentSigner, IJsonSerializer jsonSerializer)
        {
            _Configuration = configuration;
            _ContentSigner = contentSigner;
            _JsonSerializer = jsonSerializer;
        }

        public async Task<IActionResult> Execute()
        {
            var db2 = new CreateWorkflowDatabase(_Configuration);
            await db2.Execute();
            await db2.AddExampleContent();

            var db3 = new CreateContentDatabase(_Configuration, new StandardUtcDateTimeProvider(), _ContentSigner, _JsonSerializer);
            await db3.Execute();
            await db3.AddExampleContent();

            Console.WriteLine("Completed.");

            return new OkResult();
        }
    }
}
