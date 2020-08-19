// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Icc
{
    [TestClass]
    public class OriginBuilderTests
    {
        [DataRow("https://test.coronamelder-portal.nl//")]
        [DataRow("https://test.coronamelder-portal.nl/")]
        [DataRow("https://test.coronamelder-portal.nl")]
        [DataRow("https://coronamelder-portal.nl/")]
        [DataRow("https://coronamelder-portal.nl/testendpoint")]
        [DataRow("https://acceptatie.coronamelder-portal.nl")]
        [DataRow("http://localhost:4200/")]
        [TestMethod]
        public void CheckVariableBaseUrlSupport(string testFrontendBaseUrl)
        {
            // Assemble
            var builder = new OriginBuilder(testFrontendBaseUrl);

            // Assert
            var result = builder.getOrigin();
            var expected = typeof(string);
            // Act
            Assert.IsInstanceOfType(result, expected);
        }

        [DataRow("https://test.coronamelder-portal.nl//")]
        [DataRow("https://test.coronamelder-portal.nl/")]
        [DataRow("https://test.coronamelder-portal.nl")]
        [DataRow("https://coronamelder-portal.nl/")]
        [DataRow("https://coronamelder-portal.nl/testendpoint")]
        [DataRow("https://acceptatie.coronamelder-portal.nl")]
        [DataRow("http://localhost:4200/")]
        [TestMethod]
        public void ShouldNotEndWithEndpoint(string testFrontendBaseUrl)
        {
            // Assemble
            var builder = new OriginBuilder(testFrontendBaseUrl);

            // Assert
            var testUri = new Uri(testFrontendBaseUrl);
            var origin = builder.getOrigin();


            var result = origin.EndsWith(testUri.Host) || origin.EndsWith(testUri.Port.ToString());

            // Act
            Assert.IsTrue(result);
        }
    }
}