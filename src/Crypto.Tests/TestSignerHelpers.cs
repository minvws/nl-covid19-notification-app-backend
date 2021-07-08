// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests
{
    public static class TestSignerHelpers
    {
        public static CmsSignerEnhanced CreateCmsSignerEnhanced(ILoggerFactory lf)
        {
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(
                lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            var cmsCertMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertMock.Setup(x => x.Path).Returns("TestRSA.p12");
            cmsCertMock.Setup(x => x.Password).Returns("Covid-19!"); //Not a secret.

            var cmsCertChainMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertChainMock.Setup(x => x.Path).Returns("StaatDerNLChain-EV-Expires-2022-12-05.p7b");
            cmsCertChainMock.Setup(x => x.Password).Returns(string.Empty); //Not password-protected

            return new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(cmsCertMock.Object, certProviderLogger),
                new EmbeddedResourcesCertificateChainProvider(cmsCertChainMock.Object),
                new StandardUtcDateTimeProvider());
        }

        public static EcdSaSigner CreateEcdsaSigner(ILoggerFactory lf)
        {
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(
                lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            var gaCertLoc = new Mock<IEmbeddedResourceCertificateConfig>();
            gaCertLoc.Setup(x => x.Path).Returns("TestECDSA.p12");
            gaCertLoc.Setup(x => x.Password).Returns(string.Empty); //Not a secret.

            return new EcdSaSigner(
                new EmbeddedResourceCertificateProvider(
                    gaCertLoc.Object,
                    certProviderLogger));
        }
    }
}
