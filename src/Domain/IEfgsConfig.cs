// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public interface IEfgsConfig
    {
        string BaseUrl { get; }
        bool SendClientAuthenticationHeaders { get; }
        int DaysToDownload { get; }
        int MaxBatchesPerRun { get; }
        bool UploaderEnabled { get; }
        bool DownloaderEnabled { get; }
    }
}