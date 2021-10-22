// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// For GAEN EKS Signing
    /// </summary>
    public class GASigner : EcdsaBaseSigner, IGaContentSigner
    {
        private const string SignatureAlgorithmDescription = "1.2.840.10045.4.3.2";

        public GASigner(ICertificateProvider provider, IThumbprintConfig config) : base(provider, config)
        { }

        public string SignatureOid => SignatureAlgorithmDescription;
    }
}
