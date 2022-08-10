// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests
{
    public static class TestSignerHelpers
    {
        public static IHsmSignerService CreateHsmSignerService()
        {
            var dummyContent = Encoding.ASCII.GetBytes("Signature intentionally left empty");

            var hsmServiceMock = new Mock<IHsmSignerService>();
            hsmServiceMock
                .Setup(x => x.GetCmsSignatureAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(dummyContent);

            return hsmServiceMock.Object;
        }

        public static CmsSignerEnhanced CreateCmsSignerEnhanced()
        {
            var cmsCertMock = new Mock<ICertificateChainConfig>();
            cmsCertMock.Setup(x => x.Path).Returns("TestRSA.p12");
            cmsCertMock.Setup(x => x.Password).Returns("Covid-19!"); //Not a secret.

            var cmsCertChainMock = new Mock<ICertificateChainConfig>();
            cmsCertChainMock.Setup(x => x.Path).Returns("StaatDerNLChain-EV-Expires-2022-12-05.p7b");
            cmsCertChainMock.Setup(x => x.Password).Returns(string.Empty); //Not password-protected

            var thumbprintConfigMock = new Mock<IThumbprintConfig>();

            thumbprintConfigMock.Setup(x => x.RootTrusted).Returns(It.IsAny<bool>());
            thumbprintConfigMock.Setup(x => x.Thumbprint).Returns(It.IsAny<string>());

            return new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(
                    cmsCertMock.Object,
                    new NullLogger<EmbeddedResourceCertificateProvider>()),
                    cmsCertChainMock.Object,
                    new StandardUtcDateTimeProvider(),
                    thumbprintConfigMock.Object);
        }

        public static IGaContentSigner CreateGASigner()
        {
            var gaCertLoc = new Mock<ICertificateChainConfig>();
            gaCertLoc.Setup(x => x.Path).Returns("TestECDSA.p12");
            gaCertLoc.Setup(x => x.Password).Returns(string.Empty); //Not a secret.

            var thumbprintMock = new Mock<IThumbprintConfig>();
            thumbprintMock.Setup(x => x.Thumbprint).Returns(string.Empty); //Not a secret.

            return new GASigner(
                new EmbeddedResourceCertificateProvider(
                    gaCertLoc.Object,
                    new NullLogger<EmbeddedResourceCertificateProvider>()),
                    thumbprintMock.Object);
        }

        public static IGaContentSigner CreateGAv15Signer()
        {
            var gaCertLoc = new Mock<ICertificateChainConfig>();
            gaCertLoc.Setup(x => x.Path).Returns("TestECDSA.p12");
            gaCertLoc.Setup(x => x.Password).Returns(string.Empty); //Not a secret.

            var thumbprintMock = new Mock<IThumbprintConfig>();
            thumbprintMock.Setup(x => x.Thumbprint).Returns(string.Empty); //Not a secret.

            return new GAv15Signer(
                new EmbeddedResourceCertificateProvider(
                    gaCertLoc.Object,
                    new NullLogger<EmbeddedResourceCertificateProvider>()),
                    thumbprintMock.Object);
        }
    }
}
