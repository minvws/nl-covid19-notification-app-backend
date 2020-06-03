// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Security.Cryptography;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing
{
    public class HardCodedExposureKeySetSigning : IExposureKeySetSigning
    {
        private const string SignatureAlgorithmDescription = "HardCoded";

        public HardCodedExposureKeySetSigning()
        {
        }

        public string SignatureDescription => SignatureAlgorithmDescription;

        public byte[] GetSignature(byte[] content)
        {
            using var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(content);

            var signer = ECDsa.Create();
            var privateKey = "MHcCAQEEIMrxoiydeg4MGFiC0yCrAW1iVcacKidg9ZCtyHEBe0wqoAoGCCqGSM49AwEHoUQDQgAE6VXlMAvHyd9xoOnsMujJLiptp9/dvXtVPtp9TgTa4GGJl07YxGqozgTRrSc6iVh6JG8ZStYgaIRDdjd9LMgKlQ==";
            signer.ImportECPrivateKey(Convert.FromBase64String(privateKey), out _);

            return signer.SignHash(hash);
        }
    }
}