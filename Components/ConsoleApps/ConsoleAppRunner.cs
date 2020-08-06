// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using Serilog;
using Serilog.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps
{
    public sealed class ConsoleAppRunner
    {
        private ILogger<ConsoleAppRunner>? _Logger;

        public void Execute(string[] args, Action<IServiceCollection, IConfigurationRoot> configure, Action<IServiceProvider, string[]> start)
        {
            try
            {
                var configuration = ConfigurationRootBuilder.Build();
                var serviceCollection = new ServiceCollection();
                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
                serviceCollection.AddSingleton(LoggerFactory.Create(x => x.AddSerilog()));
                configure(serviceCollection, configuration);
                var serviceProvider = serviceCollection.BuildServiceProvider();
                _Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ConsoleAppRunner>();
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainOnUnhandledException;
                start(serviceProvider, args);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private void OnCurrentDomainOnUnhandledException(object _, UnhandledExceptionEventArgs e)
        {
            _Logger.LogCritical(e.ExceptionObject.ToString());
            Environment.Exit(-1);
        }
    }
}
