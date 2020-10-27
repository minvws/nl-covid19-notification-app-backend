// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ResponsePadding
{
	public static class LoggingExtensionsResponsePadding
	{
		public static void WriteNoPaddingNeeded(this ILogger logger, int resultLength, int minimumLength)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] No padding needed as response length of {Length} is greater than the minimum of {MinimumLengthInBytes}.",
				LoggingDataResponsePadding.Name, LoggingDataResponsePadding.NoPaddingNeeded,
				resultLength, minimumLength);
		}

		public static void WriteLengthOfResponsePadding(this ILogger logger, int paddingLength)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Length of response padding:{PaddingLength}.", 
			LoggingDataResponsePadding.Name, LoggingDataResponsePadding.ResponsePaddingLength,
				paddingLength);
		}

		public static void WritePaddingContent(this ILogger logger, string? padding)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Response padding:{Padding}",
				LoggingDataResponsePadding.Name, LoggingDataResponsePadding.ResponsePaddingContent,
				padding);
		}

		public static void WritePaddingAdded(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			
			logger.LogInformation("[{name}/{id}] Added padding to the response.",
				LoggingDataResponsePadding.Name, LoggingDataResponsePadding.PaddingAdded);
		}

	}
}
