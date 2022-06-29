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
            services.AddDbContext<WorkflowDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<DkSourceDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DkSource))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<ContentDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Content))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<IksInDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksIn))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<IksOutDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<IksPublishingJobDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksPublishing))
                    .UseSnakeCaseNamingConvention());
            
            services.AddDbContext<EksPublishingJobDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.EksPublishing))
                    .UseSnakeCaseNamingConvention());
        }
    }
}
