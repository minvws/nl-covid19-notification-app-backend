// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.ConsoleApps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

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
            services.GetRequiredService<PublishContentCommand>().ExecuteAsync(args).GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddScoped(x => x.CreateDbContext(y => new ContentDbContext(y), DatabaseConnectionStringNames.Content, false));
            services.AddTransient<Func<ContentDbContext>>(x => x.GetRequiredService<ContentDbContext>);

            services.AddTransient<PublishContentCommand>();

            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();

            services.AddTransient<ZippedSignedContentFormatter>();
            services.AddTransient<IContentSigner, DummyCmsSigner>();

            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();

            services.AddSingleton<PublishContentLoggingExtensions>();

            services.PublishContentForV3Startup();
        }
    }
}
