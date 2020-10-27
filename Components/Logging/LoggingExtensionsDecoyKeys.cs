using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DecoyKeys
{
	public static class LoggingExtensionsDecoyKeys
	{	
		public static void WriteStartDecoy(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			logger.LogInformation("[{name}/{id}] POST triggered.", LoggingDataDecoy.Name, LoggingDataDecoy.Start);
		}
	}
}
