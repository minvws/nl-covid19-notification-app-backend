// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PublishContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using System;

namespace PublishContent
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
            return 0;
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<PublishContentCommand>().Execute(args).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddScoped(x => DbContextStartup.Content(x, false));

            services.AddTransient<PublishContentCommand>();
            services.AddTransient<ContentDatabaseCreateCommand>();

            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();

            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<IContentSigner, DummyCmsSigner>();

            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();

            services.AddSingleton<PublishContentLoggingExtensions>();

            services.DummySignerStartup();
            services.PublishContentForV3Startup();
        }
    }
}
