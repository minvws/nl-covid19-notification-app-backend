// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass()]
    public class LuhnModNValidatorTests
    {
        [DataRow("H6SLNG9WUZ", true)]
        [DataRow("Z7BWEDFPND", true)]
        [DataRow("E4CWJGEUSZ", true)]
        [DataRow("HKV6XH4N73", true)]
        [DataRow("86HYKMZYXZ", true)]
        [DataRow("N3BBSPN5FB", true)]
        [DataRow("VR2NVH7NKH", true)]
        [DataRow("MJTTKNCBFA", true)]
        [DataRow("GNRWW4MJJF", true)]
        [DataRow("H6S-NG9WUZ", false)]
        [DataRow("Z7B9EDFPND", false)]
        [DataRow("E4CWEGEUSZ", false)]
        [DataRow("HKV6X54N75", false)]
        [DataRow("86HYK@ZYXZ", false)]
        [DataRow("N3BBS!N5FB", false)]
        [DataRow("", false)]
        [DataRow("MJTCBFA", false)]
        [DataRow(null, false)]
        [DataTestMethod]
        public void ValidateTest(string value, bool expected)
        {
            Assert.AreEqual(expected, new LuhnModNValidator(new LuhnModNConfig()).Validate(value));
            Assert.AreEqual(expected, new KeysFirstAuthorisationTokenLuhnModNValidator(new LuhnModNConfig()).IsValid(value));
        }

        [TestMethod]
        public void GenerateTest()
        {
            var c = new LuhnModNConfig();
            var r = new Random(123); //Sub in a crypto method if desired.
            var g = new LuhnModNGenerator(c);
            var result = g.Next(x => r.Next(x));
            var v = new LuhnModNValidator(c);
            Assert.IsTrue(v.Validate(result));
        }
    }
}