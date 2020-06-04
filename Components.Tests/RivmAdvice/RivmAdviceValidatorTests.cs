// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.RivmAdvice
{
    [TestClass()]
    public class RivmAdviceValidatorTests
    {
        [TestMethod()]
        public void IsBase64Test()
        {
            var thing = Convert.ToBase64String(Encoding.UTF8.GetBytes("Klingons off the starboard bow!"));
            Assert.IsTrue(RivmAdviceValidator.IsBase64(thing));
        }

        [TestMethod()]
        public void DoesCultureExistTest()
        {
            Assert.IsTrue(RivmAdviceValidator.CultureExists("en"));
            Assert.IsTrue(RivmAdviceValidator.CultureExists("en-US"));
            Assert.IsTrue(RivmAdviceValidator.CultureExists("nl"));
            Assert.IsTrue(RivmAdviceValidator.CultureExists("nl-NL"));
            Assert.IsTrue(RivmAdviceValidator.CultureExists("nl-BE"));
            Assert.IsTrue(RivmAdviceValidator.CultureExists("fr-BE"));
        }
    }
}