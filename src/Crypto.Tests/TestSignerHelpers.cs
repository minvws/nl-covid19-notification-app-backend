
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
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            //Repeated
            var cmsCertMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertMock.Setup(x => x.Path).Returns("TestRSA.p12");
            cmsCertMock.Setup(x => x.Password).Returns("Covid-19!"); //Not a secret.

            var cmsCertChainMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertChainMock.Setup(x => x.Path).Returns("StaatDerNLChain-Expires2020-08-28.p7b");
            cmsCertChainMock.Setup(x => x.Password).Returns(string.Empty);

            return new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(cmsCertMock.Object, certProviderLogger),
                new EmbeddedResourcesCertificateChainProvider(cmsCertChainMock.Object), //Not a secret.
                new StandardUtcDateTimeProvider()
            );
            //End repeated
        }
    }
}
