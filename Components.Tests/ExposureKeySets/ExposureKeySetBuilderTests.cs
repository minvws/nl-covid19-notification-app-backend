// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    public class ExposureKeySetBuilderTests
    {
        [Theory]
        [InlineData(500, 123)]
        [InlineData(1000, 123)]
        [InlineData(2000, 123)]
        [InlineData(3000, 123)]
        [InlineData(10000, 123)]
        public void Build(int keyCount, int seed)
        {
            //Arrange
            var lf = new LoggerFactory();
            var dtp = new StandardUtcDateTimeProvider();
            
            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                new EcdSaSigner(
                    new EmbeddedResourceCertificateProvider(
                        new HardCodedCertificateLocationConfig("TestECDSA.p12", ""),
                        lf.CreateLogger<EmbeddedResourceCertificateProvider>())),
                new CmsSignerEnhanced(
                    new EmbeddedResourceCertificateProvider(
                        new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"), 
                        lf.CreateLogger<EmbeddedResourceCertificateProvider>()),
                    new EmbeddedResourcesCertificateChainProvider(
                        new HardCodedCertificateLocationConfig("StaatDerNLChain-Expires2020-08-28.p7b", "")),
                    dtp
                    ),
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                lf.CreateLogger<EksBuilderV1>());

            //Act
            var result = sut.BuildAsync(GetRandomKeys(keyCount, seed)).GetAwaiter().GetResult();
            Trace.WriteLine($"{keyCount} keys = {result.Length} bytes.");
            
            //Assert
            Assert.True(result.Length > 0);

            using (var fs = new FileStream("EKS.zip", FileMode.Create, FileAccess.Write))
            {
                fs.Write(result, 0, result.Length);
            }
        }

        [Fact]
        public void EksBuilderV1WithDummy_NLSigHasDummyText()
		{
            //Arrange
            int KeyCount = 500;
            var lf = new LoggerFactory();
            var dtp = new StandardUtcDateTimeProvider();
            var dummySigner = new DummyCmsSigner();

            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                new EcdSaSigner(
                    new EmbeddedResourceCertificateProvider(
                        new HardCodedCertificateLocationConfig("TestECDSA.p12", ""),
                        lf.CreateLogger<EmbeddedResourceCertificateProvider>())
                    ),
                dummySigner,
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                lf.CreateLogger<EksBuilderV1>());

            //Act
            var result = sut.BuildAsync(GetRandomKeys(KeyCount, 123)).GetAwaiter().GetResult();

            //Assert
            using var zipFileInMemory = new MemoryStream();
            zipFileInMemory.Write(result, 0, result.Length);
            using (var zipFileContent = new ZipArchive(zipFileInMemory, ZipArchiveMode.Read, false))
            {
                var NlSignature = zipFileContent.ReadEntry(ZippedContentEntryNames.NLSignature);
                Assert.NotNull(NlSignature);
                Assert.Equal(NlSignature, dummySigner.DummyContent);
            }
        }

        private TemporaryExposureKeyArgs[] GetRandomKeys(int workflowCount, int seed)
        {
            var random = new Random(seed);
            var workflowKeyValidatorConfig = new DefaultTekValidatorConfig();
            var workflowValidatorConfig = new DefaultTekListValidationConfig();

            var result = new List<TemporaryExposureKeyArgs>(workflowCount * workflowValidatorConfig.TemporaryExposureKeyCountMax);
            var keyBuffer = new byte[workflowKeyValidatorConfig.KeyDataLength];

            for (var i = 0; i < workflowCount; i++)
            {
                var keyCount = 1 + random.Next(workflowValidatorConfig.TemporaryExposureKeyCountMax - 1);
                var keys = new List<TemporaryExposureKeyArgs>(keyCount);

                for (var j = 0; j < keyCount; j++)
                {
                    random.NextBytes(keyBuffer);
                    keys.Add(new TemporaryExposureKeyArgs
                    {
                        KeyData = keyBuffer,
                        RollingStartNumber = workflowKeyValidatorConfig.RollingPeriodMin + j,
                        RollingPeriod = 11,
                        TransmissionRiskLevel = TransmissionRiskLevel.Medium
                    });
                }
                result.AddRange(keys);

            }

            return result.ToArray();
        }
    }
}