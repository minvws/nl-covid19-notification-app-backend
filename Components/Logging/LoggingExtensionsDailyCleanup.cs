// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DailyCleanup
{
	public static class LoggingExtensionsDailyCleanup
	{
		public static void WriteStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.Start);
		}

		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Finished.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.Finished);
		}

		public static void WriteEksEngineStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - EKS engine run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.EksEngineStarting);
		}

		public static void WriteManifestEngineStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Manifest engine run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.ManifestEngineStarting);
		}

		public static void WriteDailyStatsCalcStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Calculating daily stats starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.DailyStatsCalcStarting);
		}

		public static void WriteManiFestCleanupStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Manifests run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.ManiFestCleanupStarting);
		}

		public static void WriteEksCleanupStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKS run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.EksCleanupStarting);
		}

		public static void WriteWorkflowCleanupStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Workflows run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.WorkflowCleanupStarting);
		}

		public static void WriteResignerStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Resigning existing v1 content.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.ResignerStarting);
		}

		public static void WriteEksV2CleanupStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKSv2 run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.EksV2CleanupStarting);
		}

		public static void WriteManifestV2CleanupStarting(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup ManifestV2 run starting.",
				LoggingDataDailyCleanup.Name, LoggingDataDailyCleanup.ManifestV2CleanupStarting);
		}
	}
}