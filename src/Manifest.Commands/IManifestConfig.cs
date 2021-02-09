// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands
{
    [Obsolete("Remove this class as soon as the Manifest Engine Mk2 is in place.")]
    public interface IManifestConfig
    {
        int KeepAliveCount { get; }
    }
}