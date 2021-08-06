// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class DbContextSetup
    {
        public static void DbContextRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow)));
            services.AddDbContext<DkSourceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.DkSource)));
            services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Content)));
            services.AddDbContext<IksInDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksIn)));
            services.AddDbContext<IksOutDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut)));
            services.AddDbContext<IksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksPublishing)));
            services.AddDbContext<EksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.EksPublishing)));
        }
    }
}
