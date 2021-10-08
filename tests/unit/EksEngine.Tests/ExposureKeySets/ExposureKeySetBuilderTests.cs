// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using Xunit;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    public class ExposureKeySetBuilderTests
    {
        private class FakeEksHeaderInfoConfig : IEksHeaderInfoConfig
        {
            public string AppBundleId => "nl.rijksoverheid.en";
            public string VerificationKeyId => "ServerNL";
            public string VerificationKeyVersion => "v1";
        }

        //y = 4.3416x + 715.24
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
            var eksBuilderV1Logger = new EksBuilderV1LoggingExtensions(lf.CreateLogger<EksBuilderV1LoggingExtensions>());
            var dtp = new StandardUtcDateTimeProvider();

            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                TestSignerHelpers.CreateGASigner(lf),
                TestSignerHelpers.CreateGAv15Signer(lf),
                TestSignerHelpers.CreateCmsSignerEnhanced(lf),
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                eksBuilderV1Logger);

            //Act
            var (result, resultv15) = sut.BuildAsync(GetRandomKeys(keyCount, seed)).GetAwaiter().GetResult();
            Trace.WriteLine($"{keyCount} keys = {result.Length} bytes.");

            //Assert
            Assert.True(result.Length > 0);
            Assert.True(resultv15.Length > 0);

            using var fs = new FileStream("EKS.zip", FileMode.Create, FileAccess.Write);
            fs.Write(result, 0, result.Length);
            using var fsV15 = new FileStream("EKSV15.zip", FileMode.Create, FileAccess.Write);
            fsV15.Write(resultv15, 0, resultv15.Length);
        }

        [Fact]
        public void EksBuilderV1WithDummy_NLSigHasDummyText()
        {
            //Arrange
            var keyCount = 500;
            var lf = new LoggerFactory();
            var eksBuilderV1Logger = new EksBuilderV1LoggingExtensions(lf.CreateLogger<EksBuilderV1LoggingExtensions>());
            var dtp = new StandardUtcDateTimeProvider();
            var dummySigner = new DummyCmsSigner();

            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                TestSignerHelpers.CreateGASigner(lf),
                TestSignerHelpers.CreateGAv15Signer(lf),
                dummySigner,
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                eksBuilderV1Logger);

            //Act
            var (result, resultv15) = sut.BuildAsync(GetRandomKeys(keyCount, 123)).GetAwaiter().GetResult();

            //Assert
            using var zipFileInMemory = new MemoryStream();
            zipFileInMemory.Write(result, 0, result.Length);

            using var zipFileContent = new ZipArchive(zipFileInMemory, ZipArchiveMode.Read, false);
            var nlSignature = zipFileContent.ReadEntry(ZippedContentEntryNames.NlSignature);
            Assert.NotNull(nlSignature);
            Assert.Equal(nlSignature, dummySigner.DummyContent);

            //Assert V15
            using var zipFileInMemoryV15 = new MemoryStream();
            zipFileInMemoryV15.Write(resultv15, 0, resultv15.Length);

            using var zipFileContentV15 = new ZipArchive(zipFileInMemoryV15, ZipArchiveMode.Read, false);
            var nlSignatureV15 = zipFileContentV15.ReadEntry(ZippedContentEntryNames.NlSignature);
            Assert.NotNull(nlSignatureV15);
            Assert.Equal(nlSignatureV15, dummySigner.DummyContent);
        }

        private TemporaryExposureKeyArgs[] GetRandomKeys(int workflowCount, int seed)
        {
            var random = new Random(seed);
            var workflowValidatorConfig = new DefaultTekListValidationConfig();

            var result = new List<TemporaryExposureKeyArgs>(workflowCount * workflowValidatorConfig.TemporaryExposureKeyCountMax);
            var keyBuffer = new byte[UniversalConstants.DailyKeyDataByteCount];

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
                        RollingStartNumber = UniversalConstants.RollingPeriodRange.Lo + j,
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
