// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;


namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    [Obsolete("Use this class only for testing purposes")]
    public class FakeContentSigner : IContentSigner
    {
        public string SignatureOid => "+++DoNotUse+++DoNotUse+++DoNotUse+++";

        public byte[] GetSignature(byte[] content)
        {
            var result = new byte[LengthBytes];
            new Random().NextBytes(result);
            return result;
        }

        public int LengthBytes => 256 / 8;
    }
}