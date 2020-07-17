// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DatabaseProvisioningTool
{
    public class App
    {
        private readonly ILogger _Logger;
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ExposureContentDbContext _ExposureContentDbContext;
        private readonly IccBackendContentDbContext _IccBackendContentDbContext;
        private readonly IConfigurationRoot _Configuration;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly IServiceProvider _ServiceProvider;

        public App(ILogger<App> logger,
            WorkflowDbContext workflowDbContext,
            ExposureContentDbContext exposureContentDbContext,
            IccBackendContentDbContext iccBackedBackendContentDb,
            IConfigurationRoot configuration,
            IJsonSerializer jsonSerializer,
            IServiceProvider serviceProvider
            )
        {
            _Logger = logger;
            _WorkflowDbContext = workflowDbContext;
            _ExposureContentDbContext = exposureContentDbContext;
            _IccBackendContentDbContext = iccBackedBackendContentDb;
            _Configuration = configuration;
            _JsonSerializer = jsonSerializer;
            _ServiceProvider = serviceProvider;
        }

        public async Task Run(bool seed)
        {
            _Logger.LogInformation("Running...");

            if (seed)
            {
                _Logger.LogInformation("Seeding ExposureContent...");

                var certificateProvider =
                    new X509CertificateProvider(new CertificateProviderConfig(_Configuration, "ExposureKeySets:Signing:NL"), _ServiceProvider.GetService<ILogger<X509CertificateProvider>>());
                var db = new CreateContentDatabase(_ExposureContentDbContext, new StandardUtcDateTimeProvider(), new CmsSigner(certificateProvider), _JsonSerializer);
                await db.DropExampleContent();
                await db.AddExampleContent();
            }
            else
            {
                _Logger.LogInformation("Apply WorkflowDb Migrations...");
                await _WorkflowDbContext.Database.MigrateAsync();

                _Logger.LogInformation("Apply ExposureContentDb Migrations...");
                await _ExposureContentDbContext.Database.MigrateAsync();

                _Logger.LogInformation("Apply ICCBackedContentDbContext Migrations...");
                await _IccBackendContentDbContext.Database.MigrateAsync();
            }

            _Logger.LogInformation("Completed...");
        }
    }
}
