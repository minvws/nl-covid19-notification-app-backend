// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public static class ResignExtensions
    {
        private const string NlSettingPrefix = "Certificates:NL2";
        private const string ChainPrefix = NlSettingPrefix + ":Chain";

        public static void NlResignerStartup(this IServiceCollection services)
        {
            services.AddTransient(x =>
                new NlContentResignExistingV1ContentCommand(x.GetRequiredService<NlContentResignCommand>()));

            services.AddTransient(x =>
               new NlContentResignCommand(
                    x.GetRequiredService<Func<ContentDbContext>>(),
                    new CmsSignerEnhanced(
                        new LocalMachineStoreCertificateProvider(
                            new LocalMachineStoreCertificateProviderConfig(
                                x.GetRequiredService<IConfiguration>(),
                                NlSettingPrefix),
                            x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>()),
                            
                        new EmbeddedResourcesCertificateChainProvider(
                            new EmbeddedResourceCertificateConfig(
                                x.GetRequiredService<IConfiguration>(),
                                ChainPrefix)),
                        x.GetRequiredService<IUtcDateTimeProvider>()),

                    x.GetRequiredService<ResignerLoggingExtensions>()));

        }
    }
}