// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class RemoveExpiredIksInCommandLoggingExtensions
    {
        private const string Name = "RemoveExpiredIksIn";
        private const int Base = LoggingCodex.RemoveExpiredIksIn;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private readonly ILogger _Logger;

        public RemoveExpiredIksInCommandLoggingExtensions(ILogger<RemoveExpiredIksInCommandLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _Logger.LogInformation("[{name}/{id}] Begin removing expired IksIn.",
                Name, Start);
        }

        public void WriteNumberDeleted(int numberDeleted)
        {
            _Logger.LogInformation("[{name}/{id}] Number of IksIn to deleted: {numberDeleted}",
                Name, Start, numberDeleted);
        }

        public void WriteEnd()
        {
            _Logger.LogInformation("[{name}/{id}] Finished removing expired IksIn.",
                Name, Finished);
        }
    }
}