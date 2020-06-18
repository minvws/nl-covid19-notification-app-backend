// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ProvisionDatabasesCommandIcc
    {
        private readonly IConfiguration _Configuration;

        public ProvisionDatabasesCommandIcc(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public async Task<IActionResult> Execute()
        {
            var db = new CreateIccDatabase(_Configuration);
            await db.Execute();

            Console.WriteLine("Icc ProvisionDB Completed.");

            return new OkResult();
        }
    }
}
