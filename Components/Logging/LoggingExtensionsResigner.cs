// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Resigner
{
	public static class LoggingExtensionsResigner
	{
		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Re-signing complete.",
				LoggingDataResigner.Name, LoggingDataResigner.Finished);
		}

		public static void WriteCertNotSpecified(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Certificate for re-signing not specified in settings. Re-signing will not run.",
				LoggingDataResigner.Name, LoggingDataResigner.CertNotSpecified);
		}

		public static void WriteReport(this ILogger logger, ContentEntity[]? reportcontent)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			var report = new StringBuilder();
			report.AppendLine($"Re-signing {reportcontent.Length} items:");
			
			foreach (var entry in reportcontent)
			{
				report.AppendLine($"PK:{entry.Id} PublishingId:{entry.PublishingId} Created:{entry.Created:O} Release:{entry.Release:O}");
			}

			logger.LogInformation("[{name}/{id}] {report}.",
				LoggingDataResigner.Name, LoggingDataResigner.Report,
				report.ToString());
		}
	}
}
