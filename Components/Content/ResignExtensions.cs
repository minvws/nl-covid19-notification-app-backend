// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public static class ResignExtensions
    {
        private const string NlSettingPrefix = "Certificates:NL2";
        private const string ChainPrefix = NlSettingPrefix + ":Chain";

        public static void NlResignerStartup(this IServiceCollection services)
        {
            services.AddTransient(
                x => new NlContentResignExistingV1ContentCommand(x.GetRequiredService<NlContentResignCommand>(), 
                    new LocalMachineStoreCertificateProviderConfig(x.GetRequiredService<IConfiguration>(), NlSettingPrefix), 
                    x.GetRequiredService<ILogger<NlContentResignExistingV1ContentCommand>>()));

            services.AddTransient(
                x => new NlContentResignCommand(
                        x.GetRequiredService<Func<ContentDbContext>>(),
                        new CmsSignerEnhanced(
                            new LocalMachineStoreCertificateProvider(new LocalMachineStoreCertificateProviderConfig(x.GetRequiredService<IConfiguration>(), NlSettingPrefix), x.GetRequiredService<ILogger<LocalMachineStoreCertificateProvider>>()),
                            new EmbeddedResourcesCertificateChainProvider(new StandardCertificateLocationConfig(x.GetRequiredService<IConfiguration>(), ChainPrefix)),
                            x.GetRequiredService<IUtcDateTimeProvider>()),
                        x.GetRequiredService<ILogger<NlContentResignCommand>>()));

        }
    }
}