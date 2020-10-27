// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksBuilderV1
{
	public static class LoggingExtensionsEksBuilderV1
	{
		public static void WriteNlSig(this ILogger logger, byte[]? sig)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] NL Sig: {NlSig}.",
				LoggingDataEksBuilderV1.Name, LoggingDataEksBuilderV1.NlSig,
				Convert.ToBase64String(sig));
		}

		public static void WriteGaenSig(this ILogger logger, byte[]? sig)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] GAEN Sig: {GaenSig}.",
				LoggingDataEksBuilderV1.Name, LoggingDataEksBuilderV1.GaenSig,
				Convert.ToBase64String(sig));
		}
	}
}