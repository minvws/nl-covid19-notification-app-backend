// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class EfgsConfig : AppSettingsReader, IEfgsConfig
    {
        public EfgsConfig(IConfiguration config, string? prefix = "Efgs") : base(config, prefix) { }
        public string BaseUrl => GetConfigValue(nameof(BaseUrl), "http://localhost:8080");
        public bool SendClientAuthenticationHeaders => GetConfigValue(nameof(SendClientAuthenticationHeaders), false);
        public int DaysToDownload => GetConfigValue(nameof(DaysToDownload), 7);
        public int MaxBatchesPerRun => GetConfigValue(nameof(MaxBatchesPerRun), 10);
        public bool UploaderEnabled => GetConfigValue(nameof(UploaderEnabled), true);
        public bool DownloaderEnabled => GetConfigValue(nameof(DownloaderEnabled), true);
    }
}