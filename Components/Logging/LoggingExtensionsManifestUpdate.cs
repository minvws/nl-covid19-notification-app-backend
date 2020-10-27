// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ManifestUpdateCommand
{
	public static class LoggingExtensionsManifestUpdateCommand
	{
		public static void WriteStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Manifest updating.",
				LoggingDataManifestUpdateCommand.Name, LoggingDataManifestUpdateCommand.Start);
		}

		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Manifest updated.",
				LoggingDataManifestUpdateCommand.Name, LoggingDataManifestUpdateCommand.Finished);
		}

		public static void WriteUpdateNotRequired(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Manifest does NOT require updating.",
				LoggingDataManifestUpdateCommand.Name, LoggingDataManifestUpdateCommand.UpdateNotRequired);
		}
	}
}