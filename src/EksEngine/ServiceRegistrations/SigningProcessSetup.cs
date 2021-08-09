// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.ServiceRegistrations
{
    public static class SigningProcessSetup
    {
        public static void SigningProcessRegistration(this IServiceCollection services)
        {
            // Batch Job
            services.AddTransient<SigningBatchJob>();

            // Commands
            services.NlResignerStartup();
            services.DummySignerStartup();
        }
    }
}
