// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Configs
{
    public interface IThumbprintConfig
    {
        bool Valid { get; }
        string Thumbprint { get; }
        bool RootTrusted { get; }
    }
}