// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
	public class DecoyKeysLoggingExtensions
	{
        private const string Name = "Decoykeys(PostSecret)";
		private const int Base = LoggingCodex.Decoy;

		private const int Start = Base;
		private const int Delay = Base + 1;

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

		public void WriteDelaying(int delayMs)
		{
			_Logger.LogDebug("[{name}/{id}] Delaying for {delayMs} milliseconds", 
				Name, Delay,
				delayMs);
		}
	}
}
