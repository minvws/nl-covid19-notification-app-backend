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
        private const int RegisterTime = Base + 1;
        private const int CreateDelay = Base + 2;

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

        public void WriteTimeRegistered(int entryNr, double entryTime, double entryMean, double entryStDev)
        {
            _Logger.LogDebug("[{name}/{id}] Entry no. {entryNr} registered. Time: {entryTime:F2}. Mean: {entryMean:F2}, Stdev: {entryStDev:F2}]",
                Name, RegisterTime,
                entryNr, entryTime, entryMean, entryStDev);
        }

        public void WriteGeneratingDelay(TimeSpan delayMs)
        {
            _Logger.LogDebug("[{name}/{id}] Delaying for {delayMs:F2} milliseconds",
                Name, CreateDelay,
                delayMs.TotalMilliseconds);
        }
    }
}
