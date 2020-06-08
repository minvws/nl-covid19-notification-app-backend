// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ProvisionDatabasesCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _ContextProvider;
        private readonly IDbContextProvider<WorkflowDbContext> _WorkFlowProvider;

        public ProvisionDatabasesCommand(
            IDbContextProvider<ExposureContentDbContext> contextProvider, 
            IDbContextProvider<WorkflowDbContext> workFlowProvider)
        {
            _ContextProvider = contextProvider;
            _WorkFlowProvider = workFlowProvider;
        }

        public async Task<IActionResult> Execute()
        {
                var db2 = new CreateWorkflowDatabase(_WorkFlowProvider);
                await db2.Execute();

                var db3 = new CreateContentDatabase(_ContextProvider, new Sha256PublishingId(new HardCodedExposureKeySetSigning()));
                await db3.Execute();
                await db3.AddExampleContent();
                
                Console.WriteLine("Completed.");

                return new OkResult();
        }
    }
}
