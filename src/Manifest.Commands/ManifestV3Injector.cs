// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
	public static class ManifestV3Injector
	{
		public static void ManifestForV3Startup(this IServiceCollection services)
		{
			services.AddTransient<ManifestBuilderV3>();

			services.AddTransient<Func<IContentEntityFormatter>>(x =>
				() => new StandardContentEntityFormatter(
					new ZippedSignedContentFormatter(
						SignerConfigStartup.BuildEvSigner(
							x.GetRequiredService<IConfiguration>(),
							x.GetRequiredService<LocalMachineStoreCertificateProviderLoggingExtensions>(),
							x.GetRequiredService<IUtcDateTimeProvider>())),
					x.GetRequiredService<IPublishingIdService>(),
					x.GetRequiredService<IJsonSerializer>()
				));
		}
	}
}
