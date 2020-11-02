using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DecoyKeys
{
	public static class LoggingExtensionsDecoyKeys
	{
        private const string Name = "Decoykeys(PostSecret)";
		private const int Base = LoggingCodex.Decoy;

		private const int Start = Base; //Just the one then.

		public static void WriteStartDecoy(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			logger.LogInformation("[{name}/{id}] POST triggered.", Name, Start);
		}
	}
}
