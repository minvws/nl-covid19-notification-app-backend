// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// May be 2 of these - Token First, Keys First.
    /// </summary>
    public interface ITekSource
    {
        SourceItem[] Read();
        void Delete(int[] kf, int[] kl);
    }
}