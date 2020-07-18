// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    public abstract class AppFunctionBase
    {
        protected IConfigurationRoot Configuration { get; private set; }

        protected void SetConfig(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (Configuration != null)
                return;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}