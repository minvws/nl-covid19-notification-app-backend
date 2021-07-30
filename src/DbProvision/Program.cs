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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
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
            services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow)));
            services.AddDbContext<DkSourceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.DkSource)));
            services.AddDbContext<ContentDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Content)));
            services.AddDbContext<IksInDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksIn)));
            services.AddDbContext<IksOutDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut)));
            services.AddDbContext<IksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.IksPublishing)));
            services.AddDbContext<EksPublishingJobDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.EksPublishing)));
            services.AddDbContext<StatsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.Stats)));
            services.AddDbContext<DataProtectionKeysDbContext>(options => options.UseSqlServer(configuration.GetConnectionString(DatabaseConnectionStringNames.DataProtectionKeys)));

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<DbProvisionLoggingExtensions>();
            services.AddSingleton<PublishContentLoggingExtensions>();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddTransient<DatabaseProvisioner>();
            services.AddTransient<ContentPublisher>();
            services.AddTransient<ContentValidator>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<ContentInsertDbCommand>();
            services.AddTransient<IContentSigner, DummyCmsSigner>();

            services.PublishContentForV3Startup();
        }
    }
}
