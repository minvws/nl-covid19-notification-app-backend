// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
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
            
            if (args.Length < 2) return; // No ContentPublisher arguments given

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
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<DbProvisionLoggingExtensions>();
            services.AddSingleton<PublishContentLoggingExtensions>();

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();
            services.AddScoped(x => x.CreateDbContext(y => new WorkflowDbContext(y), DatabaseConnectionStringNames.Workflow, false));
            services.AddScoped(x => x.CreateDbContext(y => new ContentDbContext(y), DatabaseConnectionStringNames.Content, false));
            services.AddScoped(x => x.CreateDbContext(y => new EksPublishingJobDbContext(y), DatabaseConnectionStringNames.EksPublishing, false));
            services.AddScoped(x => x.CreateDbContext(y => new DataProtectionKeysDbContext(y), DatabaseConnectionStringNames.DataProtectionKeys, false));
            services.AddScoped(x => x.CreateDbContext(y => new StatsDbContext(y), DatabaseConnectionStringNames.Stats, false));
            services.AddScoped(x => x.CreateDbContext(y => new DkSourceDbContext(y), DatabaseConnectionStringNames.DkSource, false));
            services.AddScoped(x => x.CreateDbContext(y => new IksInDbContext(y), DatabaseConnectionStringNames.IksIn, false));
            services.AddScoped(x => x.CreateDbContext(y => new IksOutDbContext(y), DatabaseConnectionStringNames.IksOut, false));
            services.AddScoped(x => x.CreateDbContext(y => new IksPublishingJobDbContext(y), DatabaseConnectionStringNames.IksPublishing, false));

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
