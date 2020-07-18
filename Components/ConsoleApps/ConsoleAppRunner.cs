// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps
{
    public sealed class ConsoleAppRunner
    {
        public void Execute(string[] args, Action<IServiceCollection, IConfigurationRoot> configure, Action<IServiceProvider> start)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();

            // Add the framework services
            var serviceCollection = new ServiceCollection();
            configure(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<ConsoleAppRunner>>();
            AppDomain.CurrentDomain.UnhandledException += (o,e) => logger.LogCritical(e.ExceptionObject.ToString()); ;
            start(serviceProvider);
        }
    }
}