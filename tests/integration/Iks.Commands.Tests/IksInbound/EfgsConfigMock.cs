// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound
{
    /// <summary>
    /// Stub for my config
    /// </summary>
    class EfgsConfigMock : IEfgsConfig
    {
        public string BaseUrl => "";
        public bool SendClientAuthenticationHeaders => true;
        public int DaysToDownload => 1;
        public int MaxBatchesPerRun => 10;
        public bool UploaderEnabled => true;
        public bool DownloaderEnabled => true;
    }
}
