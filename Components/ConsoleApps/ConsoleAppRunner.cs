// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps
{
    public sealed class ConsoleAppRunner
    {
        public void Execute(string[] args, Action<IServiceCollection, IConfigurationRoot> configure, Action<IServiceProvider, string[]> start)
        {
            var configuration = ConfigurationRootBuilder.Build();
            // Add the framework services
            var serviceCollection = new ServiceCollection();
            configure(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<ConsoleAppRunner>>();
            AppDomain.CurrentDomain.UnhandledException += (o,e) => logger.LogCritical(e.ExceptionObject.ToString());
            start(serviceProvider, args);
        }
    }
}