// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DbProvision
{
	public static class LoggingExtensionsDbProvision
	{
		public static void WriteStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Start.",
				LoggingDataDbProvision.Name, LoggingDataDbProvision.Start);
		}

		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Complete.",
				LoggingDataDbProvision.Name, LoggingDataDbProvision.Finished);
		}

		public static void WriteWorkFlowDb(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Workflow...",
				LoggingDataDbProvision.Name, LoggingDataDbProvision.WorkflowDb);
		}

		public static void WriteContentDb(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Content...",
				LoggingDataDbProvision.Name, LoggingDataDbProvision.ContentDb);
		}

		public static void WriteJobDb(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Job...",
				LoggingDataDbProvision.Name, LoggingDataDbProvision.JobDb);
		}
	}
}
