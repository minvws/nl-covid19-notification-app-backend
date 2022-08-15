// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class SigningProcessSetup
    {
        public static void SigningProcessRegistration(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IHsmSignerConfig, HsmSignerConfig>(
                x => new HsmSignerConfig(configuration, "Certificates:HsmSigner"));
            services.AddHttpClient<IHsmSignerService, HsmSignerService>();
        }
    }
}
