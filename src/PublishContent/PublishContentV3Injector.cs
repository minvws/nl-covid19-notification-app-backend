// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace PublishContent
{
    public static class PublishContentV3Injector
    {
        public static void PublishContentForV3Startup(this IServiceCollection services)
        {
            services.AddSingleton<LocalMachineStoreCertificateProviderLoggingExtensions>();

            services.AddTransient<Func<ContentInsertDbCommand>>(x =>
                () => new ContentInsertDbCommand(
                        x.GetRequiredService<ContentDbContext>(),
                        x.GetRequiredService<IUtcDateTimeProvider>(),
                        x.GetRequiredService<IPublishingIdService>(),
                        new ZippedSignedContentFormatter(
                            SignerConfigStartup.BuildEvSigner(
                                x.GetRequiredService<IConfiguration>(),
                                x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>(),
                                x.GetRequiredService<IUtcDateTimeProvider>())))
                );
        }
    }
}
