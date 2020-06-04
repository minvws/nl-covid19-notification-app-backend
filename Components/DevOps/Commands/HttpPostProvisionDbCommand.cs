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

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Commands
{
    public class HttpPostProvisionDbCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _contextProvider;
        private readonly IDbContextProvider<ExposureKeySetsBatchJobDbContext> _jobContextProvider;
        private readonly IDbContextProvider<WorkflowDbContext> _workFlowProvider;

        public HttpPostProvisionDbCommand(
            IDbContextProvider<ExposureContentDbContext> contextProvider, 
            IDbContextProvider<ExposureKeySetsBatchJobDbContext> jobContextProvider,
            IDbContextProvider<WorkflowDbContext> workFlowProvider)
        {
            _contextProvider = contextProvider;
            _jobContextProvider = jobContextProvider;
            _workFlowProvider = workFlowProvider;
        }

        public async Task<IActionResult> Execute()
        {
            try
            {
                var db1 = new CreateJobDatabase(_jobContextProvider);
                await db1.Execute();

                var db2 = new CreateWorkflowDatabase(_workFlowProvider);
                await db2.Execute();

                var dpProvision = new CreateContentDatabase(_contextProvider, new Sha256PublishingIdCreator(new HardCodedExposureKeySetSigning()));
                await dpProvision.Execute();
                await dpProvision.Seed();
                
                Console.WriteLine("Completed.");

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
