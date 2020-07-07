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
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DatabaseProvisioningTool
{
    internal class Program
    {
        protected Program()
        {
            
        }

        public static IConfigurationRoot Configuration { get; private set; }

        private static async Task Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();

            // add the framework services
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                Log.Information("Starting service");
                await serviceProvider.GetService<App>().Run(args);
                Log.Information("Ending service");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error running service");
                throw ex;
            }
            finally
            {
                Log.CloseAndFlush();
            }
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
