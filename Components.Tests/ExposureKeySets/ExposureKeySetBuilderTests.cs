// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Configs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using TemporaryExposureKeyArgs = NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.TemporaryExposureKeyArgs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [TestClass]
    public class ExposureKeySetBuilderTests
    {
        //y = 4.3416x + 715.24
        [DataRow(500, 123)]
        [DataRow(1000, 123)]
        [DataRow(2000, 123)]
        [DataRow(3000, 123)]
        [DataRow(10000, 123)]
        [DataTestMethod]
        public void Build(int keyCount, int seed)
        {
            var builder = new ExposureKeySetBuilderV1(new FakeExposureKeySetHeaderInfoConfig(), new EcdSaSigner(new ResourceCertificateProvider("FakeECDSA.p12")), new CmsSigner(new ResourceCertificateProvider("FakeRSA.p12")), new StandardUtcDateTimeProvider(), new GeneratedProtobufContentFormatter());

            var actual = builder.BuildAsync(GetRandomKeys(keyCount, seed)).GetAwaiter().GetResult();
            Assert.IsTrue(actual.Length > 0);
            Trace.WriteLine($"{keyCount} keys = {actual.Length} bytes.");
        }

        private TemporaryExposureKeyArgs[] GetRandomKeys(int WorkflowCount, int seed)
        {
            var random = new Random(seed);
            var WorkflowKeyValidatorConfig = new DefaultGaenTekValidatorConfig();
            var WorkflowValidatorConfig = new DefaultGeanTekListValidationConfig();

            var result = new List<TemporaryExposureKeyArgs>(WorkflowCount * WorkflowValidatorConfig.TemporaryExposureKeyCountMax);
            var keyBuffer = new byte[WorkflowKeyValidatorConfig.DailyKeyByteCount];

            for (var i = 0; i < WorkflowCount; i++)
            {

                var keyCount = 1 + random.Next(WorkflowValidatorConfig.TemporaryExposureKeyCountMax - 1);
                var keys = new List<TemporaryExposureKeyArgs>(keyCount);
                for (var j = 0; j < keyCount; j++)
                {
                    random.NextBytes(keyBuffer);
                    keys.Add(new TemporaryExposureKeyArgs
                    {
                        KeyData = keyBuffer,
                        RollingStartNumber = WorkflowKeyValidatorConfig.RollingPeriodMin + j,
                        RollingPeriod = 11,
                        TransmissionRiskLevel = 2
                    });
                }
                result.AddRange(keys);
            }
            return result.ToArray();
        }
    }
}