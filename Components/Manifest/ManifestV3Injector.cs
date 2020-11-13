// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
	public static class ManifestV3Injector
	{
		public static void ManifestForV3Startup(this IServiceCollection services)
		{
			services.AddTransient<ManifestBuilderForV3>();
			services.AddTransient<ContentEntityFormatterForV3>();

			services.NlSignerForV3Startup();
			services.AddTransient<ZippedSignedContentFormatterForV3>();
		}
	}
}
