// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public class ManifestUpdateCommandLoggingExtensions
    {
        private readonly string _name = "ManifestUpdateCommand";
        private const int Base = LoggingCodex.ManifestUpdate;
        private const int Start = Base;
        private const int Finished = Base + 99;
        private const int UpdateNotRequired = Base + 1;

        private readonly ILogger _logger;

        public ManifestUpdateCommandLoggingExtensions(ILogger<ManifestUpdateCommandLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _logger.LogInformation("[{name}/{id}] Manifest updating.",
                _name, Start);
        }

        public void WriteFinished()
        {
            _logger.LogInformation("[{name}/{id}] Manifest updated.",
                _name, Finished);
        }

        public void WriteUpdateNotRequired()
        {
            _logger.LogInformation("[{name}/{id}] Manifest does NOT require updating.",
                _name, UpdateNotRequired);
        }
    }
}
