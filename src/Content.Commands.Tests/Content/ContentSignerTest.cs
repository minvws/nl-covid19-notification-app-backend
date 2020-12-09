// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing.Signers;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests.Content
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

            var signer = new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"), certProviderLogger), //Not a secret.
                new EmbeddedResourcesCertificateChainProvider(new HardCodedCertificateLocationConfig("Resources.StaatDerNLChain-Expires2020-08-28.p7b", "")), //Not a secret.
                new StandardUtcDateTimeProvider()
                );
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
