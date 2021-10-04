// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// For signing EKS with GAEN-V15 certificate
    /// </summary>
    public class GAv15Signer : EcdsaBaseSigner, IGaContentSigner
    {
        private const string SignatureAlgorithmDescription = "1.2.840.10045.4.3.2";

        private const string GaV12Thumbprint = "Certificates:GAv15"; // Todo: path to thumbprint in config should be stored in these classes; refactor thumbprintconfigprovider to use this string

        public GAv15Signer(ICertificateProvider provider, IThumbprintConfig config) : base(provider, config)
        { }

        public string SignatureOid => SignatureAlgorithmDescription;
    }
}
