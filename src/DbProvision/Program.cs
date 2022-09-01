// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

namespace DbProvision
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
            return 0;
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<DatabaseProvisioner>().ExecuteAsync(args).GetAwaiter().GetResult();

            if (args.Length < 2)
            {
                return; // No ContentPublisher arguments given
            }

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith('-'))
                {
                    var subArgs = new[] { args[i], args[++i] }; // Get contenttype argument with value

                    services.GetRequiredService<ContentPublisher>().ExecuteAsync(subArgs).GetAwaiter().GetResult();
                }
            }
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<WorkflowDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow))
                    .UseSnakeCaseNamingConvention());

            services.AddDbContext<DiagnosisKeysDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DiagnosisKeys))
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

            services.AddDbContext<StatsDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Stats))
                    .UseSnakeCaseNamingConvention());

            services.AddDbContext<DataProtectionKeysDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DataProtectionKeys))
                    .UseSnakeCaseNamingConvention());

            services.AddDbContext<DashboardDataDbContext>(
                options => options
                    .UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DashboardData))
                    .UseSnakeCaseNamingConvention());

            services.AddSingleton<IConfiguration>(configuration);

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddTransient<DatabaseProvisioner>();
            services.AddTransient<ContentPublisher>();
            services.AddTransient<ContentValidator>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<ContentInsertDbCommand>();
        }
    }
}
