// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DatabaseProvisioningTool
{
    public static class ConfiguationRootBuilder
    {
        public static IConfigurationRoot Build()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();
        }
    }

    public static class Program
    {
        public static IConfigurationRoot Configuration { get; private set; }

        public static async Task Main(params string[] args)
        {
            Configuration = ConfiguationRootBuilder.Build();
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            AppDomain.CurrentDomain.UnhandledException += (o,e) => logger.LogCritical(e?.ExceptionObject?.ToString());
            logger.LogInformation("Starting service");
            await serviceProvider.GetService<App>().Run(args);
            logger.LogInformation("Ending service");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            ComponentsContainerHelper.RegisterDefaultServices(services);

            services.AddSeriLog(Configuration);
            services.AddSingleton(Configuration);
            services.AddTransient<App>();

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Content");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ExposureContentDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Workflow");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new WorkflowDbContext(builder.Build());
                return result;
            });

            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "Icc");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new IccBackendContentDbContext(builder.Build());
                return result;
            });
        }
    }
}
