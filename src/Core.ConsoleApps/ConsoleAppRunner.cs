// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps
{
    public sealed class ConsoleAppRunner
    {
        private ServiceProvider _ServiceProvider;

        public void Execute(string[] args, Action<IServiceCollection, IConfigurationRoot> configure, Action<IServiceProvider, string[]> start)
        {
            try
            {
                var configuration = ConfigurationRootBuilder.Build();
                var serviceCollection = new ServiceCollection();

                Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                serviceCollection.AddLogging(builder =>
                {
                    builder.AddSerilog();
                });

                Log.Debug("Created the logger");

                configure(serviceCollection, configuration);
                _ServiceProvider = serviceCollection.BuildServiceProvider();

                Log.Debug("About to start the console app");

                start(_ServiceProvider, args);
            }
            catch(Exception ex)
            {
                _ServiceProvider.GetRequiredService<ILogger<ConsoleAppRunner>>()
                    .LogCritical(ex.ToString());
            }
            finally
            {
                Log.Debug("About to close and flush the log");
                Log.CloseAndFlush();
            }
        }
    }
}
