// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing
{
    public class HardCodedExposureKeySetSigning : IExposureKeySetSigning
    {
        private const string SignatureAlgorithmDescription = "HardCoded";

        public string SignatureDescription => SignatureAlgorithmDescription;

        public byte[] GetSignature(byte[] content)
        {
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(content);

            var signer = ECDsa.Create();
            signer.GenerateKey(ECCurve.CreateFromFriendlyName("ECDSA_P256"));
            return signer.SignHash(hash);
        }
    }
}