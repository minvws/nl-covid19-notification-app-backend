// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public sealed class ConsoleAppRunner
    {
        private ILogger _Logger;
        private IConfigurationRoot _Configuration;

        public void Execute(string[] args, Action<IServiceCollection, IConfigurationRoot> configure, Action<IServiceProvider> start)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration
            _Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();

            // Add the framework services
            var serviceCollection = new ServiceCollection();
            configure(serviceCollection, _Configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _Logger = serviceProvider.GetService<ILogger>();
            AppDomain.CurrentDomain.UnhandledException += AppDomainExceptinHandler;
            start(serviceProvider);
        }

        private void AppDomainExceptinHandler(object sender, UnhandledExceptionEventArgs e)
        {
            _Logger.LogCritical(e.ExceptionObject.ToString());
        }
    }
}