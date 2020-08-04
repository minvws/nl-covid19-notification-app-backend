// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass]
    public class RandomNumberTests
    {
        [TestMethod]
        public void Valid()
        {
            var random = new StandardRandomNumberGenerator();
            const int minValue = 1;
            const int maxValue = 99999999;

            var result = random.Next(minValue, maxValue);

            Assert.IsTrue(result >= minValue);
            Assert.IsTrue(result <= maxValue);
        }

        [DataRow(1)]
        [DataRow(256)]
        [DataRow(3030)]
        [DataTestMethod]
        public void GenerateToken(int length)
        {
            var random = new LabConfirmationIdService(new StandardRandomNumberGenerator());
            var result = random.Next();
            Assert.IsTrue(result.Length == 6);

            Assert.IsTrue(random.Validate(result).Length == 0);

        }


        [TestMethod]
        public void GetZero()
        {
            var zeros = 0;
            var ones = 0;
            var r = new StandardRandomNumberGenerator();
            for (var i = 0; i < 1000; i++)
            {
                var next = r.Next(0, 1);
                Assert.IsTrue(next == 0 || next == 1);
                if (next == 0) zeros++;
                if (next == 1) ones++;
            }

            Assert.IsTrue(zeros > 0);
            Assert.IsTrue(ones > 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Min value is higher than max value")]
        public void RangeTest()
        {
            var random = new StandardRandomNumberGenerator();
            const int minValue = 8;
            const int maxValue = 5;
            random.Next(minValue, maxValue);
        } //ncrunch: no coverage
    }
}
