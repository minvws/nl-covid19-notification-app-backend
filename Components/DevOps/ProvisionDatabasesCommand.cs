// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ProvisionDatabasesCommand
    {
        private readonly IConfiguration _Configuration;

        public ProvisionDatabasesCommand(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public async Task<IActionResult> Execute()
        {
            var db2 = new CreateWorkflowDatabase(_Configuration);
            await db2.Execute();
            await db2.AddExampleContent();

            var db3 = new CreateContentDatabase(_Configuration, new StandardUtcDateTimeProvider());
            await db3.Execute();
            await db3.AddExampleContent();

            Console.WriteLine("Completed.");

            return new OkResult();
        }
    }
}
