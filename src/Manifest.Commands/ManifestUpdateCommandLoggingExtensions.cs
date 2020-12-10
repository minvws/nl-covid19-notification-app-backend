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
        private string Name = "ManifestUpdateCommand";
        private const int Base = LoggingCodex.ManifestUpdate;
        private const int Start = Base;
        private const int Finished = Base + 99;
        private const int UpdateNotRequired = Base + 1;

        private readonly ILogger _Logger;

        public ManifestUpdateCommandLoggingExtensions(ILogger<ManifestUpdateCommandLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _Logger.LogInformation("[{name}/{id}] Manifest updating.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _Logger.LogInformation("[{name}/{id}] Manifest updated.",
                Name, Finished);
        }

        public void WriteUpdateNotRequired()
        {
            _Logger.LogInformation("[{name}/{id}] Manifest does NOT require updating.",
                Name, UpdateNotRequired);
        }
    }
}