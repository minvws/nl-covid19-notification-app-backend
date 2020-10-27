// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.SigTestFileCreator
{
	public static class LoggingExtensionsSigTestFileCreator
	{
		public static void WriteStart(this ILogger logger, DateTime time)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Key presence Test started ({Time}).",
				LoggingDataSigTestFileCreator.Name, LoggingDataSigTestFileCreator.Start,
				time);
		}

		public static void WriteFinished(this ILogger logger, string outputLocation)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Key presence test complete.\nResults can be found in: {OutputLocation}.",
				LoggingDataSigTestFileCreator.Name, LoggingDataSigTestFileCreator.Finished,
				outputLocation);
		}

		public static void WriteNoElevatedPrivs(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] The test was started WITHOUT elevated privileges - errors may occur when signing content.",
				LoggingDataSigTestFileCreator.Name, LoggingDataSigTestFileCreator.NoElevatedPrivs);
		}

		public static void WriteBuildingResultFile(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Building EKS-engine resultfile.",
				LoggingDataSigTestFileCreator.Name, LoggingDataSigTestFileCreator.BuildingResultFile);
		}

		public static void WriteSavingResultfile(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Saving EKS-engine resultfile.",
				LoggingDataSigTestFileCreator.Name, LoggingDataSigTestFileCreator.SavingResultFile);
		}
	}

}
