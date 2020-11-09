using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DecoyKeys
{
	public class DecoyKeysLoggingExtensions
	{
        private const string Name = "Decoykeys(PostSecret)";
		private const int Base = LoggingCodex.Decoy;

		private const int Start = Base;

		private readonly ILogger _Logger;

		public DecoyKeysLoggingExtensions(ILogger<DecoyKeysLoggingExtensions> logger)
		{
			_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void WriteStartDecoy()
		{
			_Logger.LogInformation("[{name}/{id}] POST triggered.",
				Name, Start);
		}
	}
}
