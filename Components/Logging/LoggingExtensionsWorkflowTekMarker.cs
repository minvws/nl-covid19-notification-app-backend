// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.MarkWorkFlowTeksAsUsed
{
	public static class LoggingExtensionsMarkWorkFlowTeksAsUsed
	{
		public static void WriteMarkingAsPublished(this ILogger logger, int zapcount, int total)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Marking as Published - Count:{Count}, Running total:{RunningTotal}.",
				LoggingDataMarkWorkFlowTeksAsUsed.Name, LoggingDataMarkWorkFlowTeksAsUsed.MarkAsPublished,
				zapcount, total);
		}
	}
}
