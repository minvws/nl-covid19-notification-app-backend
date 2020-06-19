// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DatabaseProvisioningTool
{
    public class App
    {
        private readonly ILogger<App> _Logger;
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ExposureContentDbContext _ExposureContentDbContext;

        public App(ILogger<App> logger, WorkflowDbContext workflowDbContext, ExposureContentDbContext exposureContentDbContext)
        {
            _Logger = logger;
            _WorkflowDbContext = workflowDbContext;
            _ExposureContentDbContext = exposureContentDbContext;
        }

        public async Task Run()
        {
            _Logger.LogInformation("Running...");
            
            _Logger.LogInformation("Apply WorkflowDb Migrations...");
            await _WorkflowDbContext.Database.MigrateAsync();
            
            
            _Logger.LogInformation("Apply ExposureContentDb Migrations...");
            await _ExposureContentDbContext.Database.MigrateAsync();

            _Logger.LogInformation("Seeding ExposureContent...");
            var db = new CreateContentDatabase(_ExposureContentDbContext, new StandardUtcDateTimeProvider(), new ContentSigner(new FakeCertificateProvider("FakeRSA.p12")));
            await db.AddExampleContent();

            _Logger.LogInformation("Completed...");
        }
    }
}
