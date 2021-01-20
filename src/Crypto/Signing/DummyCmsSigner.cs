// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// Returns a byte array that indicates no RSA-signature is created
    /// </summary>
    public class DummyCmsSigner : IContentSigner
    {
        public string SignatureOid => "OID for Dummy signer";

        public byte[] DummyContent => Encoding.ASCII.GetBytes("Signature intentionally left empty");

        public byte[] GetSignature(byte[] content)
        {
            return DummyContent;
        }

    }
}
