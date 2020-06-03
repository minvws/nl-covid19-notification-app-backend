// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass()]
    public class LuhnModNValidatorConfigTests
    {
        [TestMethod()]
        public void GoodDefaults()
        {
            new LuhnModNConfig();
        }

        [TestMethod()]
        public void GoodCustomValues()
        {
            var argle = new LuhnModNConfig("abcdef", 3);
            Assert.AreEqual(3, argle.ValueLength);
            Assert.AreEqual(6, argle.CharacterSet.Length);
            Assert.IsTrue(argle.CharacterSet.Contains('c'));
            Assert.IsTrue(!argle.CharacterSet.Contains('g'));
        }

        [TestMethod()]
        public void BadLengthLo()
        {
            Assert.ThrowsException<ArgumentException>(() => new LuhnModNConfig("abcdef", 1));
        }

        [TestMethod()]
        public void BadCharacterSet()
        {
            Assert.ThrowsException<ArgumentException>(() => new LuhnModNConfig("sdssdfsdf", 2));
        }
    }
}