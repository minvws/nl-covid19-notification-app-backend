// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing
{

    /// <summary>
    /// TODO more generic name. Scope of use is now wider.
    /// </summary>
    public interface ISigner
    {
        string SignatureDescription { get; }
        byte[] GetSignature(byte[] content);
        int LengthBytes { get; }
    }
}