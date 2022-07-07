using System;
using DbProvision;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.AspNet.DataProtection.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

namespace DbBuilder
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
            services.GetRequiredService<DatabaseBuilder>().ExecuteAsync(args).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddDbContext<WorkflowDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Workflow)));
            services.AddDbContext<DkSourceDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DkSource)));
            services.AddDbContext<ContentDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Content)));
            services.AddDbContext<IksInDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksIn)));
            services.AddDbContext<IksOutDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksOut)));
            services.AddDbContext<IksPublishingJobDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.IksPublishing)));
            services.AddDbContext<EksPublishingJobDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.EksPublishing)));
            services.AddDbContext<StatsDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.Stats)));
            services.AddDbContext<DataProtectionKeysDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(DatabaseConnectionStringNames.DataProtectionKeys)));

            services.AddSingleton<IConfiguration>(configuration);

            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddTransient<DatabaseBuilder>();
        }
    }
}
