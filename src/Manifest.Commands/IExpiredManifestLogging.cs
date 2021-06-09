// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    public interface IExpiredManifestLogging
    {
        void WriteStart(int keepAliveCount);
        void WriteFinished(int zombieCount, int givenMercyCount);
        void WriteFinishedNothingRemoved();
        void WriteRemovingManifests(int zombieCount);
        void WriteRemovingEntry(string publishingId, DateTime releaseDate);
        void WriteReconciliationFailed(int reconciliationCount);
        void WriteDeletionReconciliationFailed(int deleteReconciliationCount);
    }
}
