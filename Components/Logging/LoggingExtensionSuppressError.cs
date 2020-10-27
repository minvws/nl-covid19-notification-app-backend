// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.SuppressError
{
	public static class LoggingExtensionSuppressError
	{
		public static void WriteCallFailed(this ILogger logger, ActionDescriptor actionDescriptor)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Call to {ActionDescriptor} failed, overriding response code to return 200.",
				LoggingDataSuppressError.Name, LoggingDataSuppressError.CallFailed,
				actionDescriptor);
		}

	}
}
