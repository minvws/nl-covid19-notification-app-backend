// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates
{
    /// <summary>
    /// Embedded resources or file system.
    /// </summary>
    public interface IEmbeddedResourceCertificateConfig
    {
        /// <summary>
        /// File system location or embedded resource path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// NB this is currently blank for the current NL cert chain but may not be for test certs that contain a private key.
        /// In the case of tests, the password is NOT considered a secret.
        /// </summary>
        public string Password { get; }
    }
}