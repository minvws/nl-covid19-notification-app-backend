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

        private const string GaV12Thumbprint = "Certificates:GA"; // Todo: path to thumbprint in config should be stored in these classes; refactor thumbprintconfigprovider to use this string

        public GASigner(ICertificateProvider provider, IThumbprintConfig config) : base(provider, config)
        { }

        public string SignatureOid => SignatureAlgorithmDescription;
    }
}
