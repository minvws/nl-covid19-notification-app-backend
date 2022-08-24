// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using Xunit;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;
using Microsoft.Extensions.Logging.Abstractions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    public class ExposureKeySetBuilderTests
    {
        private class FakeEksHeaderInfoConfig : IEksHeaderInfoConfig
        {
            public string AppBundleId => "nl.rijksoverheid.en";
            public string VerificationKeyId => "ServerNL";
            public string VerificationKeyVersion => "v1";
            public string VerificationKeyVersionV15 => "v2";
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
            var dtp = new StandardUtcDateTimeProvider();

            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                TestSignerHelpers.CreateHsmSignerService(),
                new NullLogger<EksBuilderV1>());

            //Act
            var result = sut.BuildAsync(GetRandomKeys(keyCount, seed)).GetAwaiter().GetResult();
            Trace.WriteLine($"{keyCount} keys = {result.Length} bytes");

            //Assert
            Assert.True(result.Length > 0);

            using var fs = new FileStream("EKS.zip", FileMode.Create, FileAccess.Write);
            fs.Write(result, 0, result.Length);
        }

        [Fact]
        public void EksBuilderV1WithDummy_NLSigHasDummyText()
        {
            //Arrange
            var keyCount = 500;
            var dtp = new StandardUtcDateTimeProvider();

            var sut = new EksBuilderV1(
                new FakeEksHeaderInfoConfig(),
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                TestSignerHelpers.CreateHsmSignerService(),
                new NullLogger<EksBuilderV1>());

            var dummyContent = Encoding.ASCII.GetBytes("Signature intentionally left empty");

            //Act
            var result = sut.BuildAsync(GetRandomKeys(keyCount, 123)).GetAwaiter().GetResult();

            //Assert
            using var zipFileInMemory = new MemoryStream();
            zipFileInMemory.Write(result, 0, result.Length);

            using var zipFileContent = new ZipArchive(zipFileInMemory, ZipArchiveMode.Read, false);
            var cmsSignature = zipFileContent.ReadEntry(ZippedContentEntryNames.CmsSignature);
            Assert.NotNull(cmsSignature);
            Assert.Equal(cmsSignature, dummyContent);
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
