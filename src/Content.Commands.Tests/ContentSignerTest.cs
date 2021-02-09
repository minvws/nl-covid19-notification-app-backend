// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    public class ContentSignerTest
    {
        private static readonly Random _Random = new Random();

        [Theory]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(2000)]
        [InlineData(3000)]
        [InlineData(10000)]
        public void Build(int length)
        {
            var lf = new LoggerFactory();
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            //Repeated
            var cmsCertMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertMock.Setup(x => x.Path).Returns("TestRSA.p12");
            cmsCertMock.Setup(x => x.Password).Returns("Covid-19!"); //Not a secret.

            var cmsCertChainMock = new Mock<IEmbeddedResourceCertificateConfig>();
            cmsCertChainMock.Setup(x => x.Path).Returns("StaatDerNLChain-Expires2020-08-28.p7b");
            cmsCertChainMock.Setup(x => x.Password).Returns(string.Empty);

            var signer = new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(cmsCertMock.Object, certProviderLogger), 
                new EmbeddedResourcesCertificateChainProvider(cmsCertChainMock.Object), //Not a secret.
                new StandardUtcDateTimeProvider()
                );
            //End repeated

            var content = Encoding.UTF8.GetBytes(CreateString(length));

            var sig = signer.GetSignature(content);

            Assert.True((sig?.Length ?? 0) != 0);
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[stringLength];

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[_Random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}
