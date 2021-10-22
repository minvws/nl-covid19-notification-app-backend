// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.ServiceRegistrations
{
    public static class SigningProcessSetup
    {
        private const string EfgsSigningSettingPrefix = "Certificates:EFGSSigning";

        public static void SigningProcessRegistration(this IServiceCollection services)
        {
            services.AddTransient<IThumbprintConfig>(
                x => new ThumbprintConfig(
                    x.GetRequiredService<IConfiguration>(),
                    EfgsSigningSettingPrefix));

            // Batch Job
            services.AddTransient<IIksSigner, EfgsCmsSigner>();
            services.AddTransient<ICertificateChainConfig, CertificateChainConfig>();
            services.AddTransient<ICertificateProvider>(
                x => new LocalMachineStoreCertificateProvider(
                    x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()));
        }
    }
}
