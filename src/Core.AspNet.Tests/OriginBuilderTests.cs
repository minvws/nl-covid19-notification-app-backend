// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet.Tests
{
    public class OriginBuilderTests
    {
        [InlineData("https://test.coronamelder-portal.nl//")]
        [InlineData("https://test.coronamelder-portal.nl/")]
        [InlineData("https://test.coronamelder-portal.nl")]
        [InlineData("https://coronamelder-portal.nl/")]
        [InlineData("https://coronamelder-portal.nl/testendpoint")]
        [InlineData("https://acceptatie.coronamelder-portal.nl")]
        [InlineData("http://localhost:4200/")]
        [Theory]
        public void CheckVariableBaseUrlSupport(string testFrontendBaseUrl)
        {
            // Assemble
            var builder = new OriginBuilder(testFrontendBaseUrl);

            // Assert
            var result = builder.GetOrigin();
            
            // Act
            Assert.IsType<string>(result);
        }

        [InlineData("https://test.coronamelder-portal.nl//")]
        [InlineData("https://test.coronamelder-portal.nl/")]
        [InlineData("https://test.coronamelder-portal.nl")]
        [InlineData("https://coronamelder-portal.nl/")]
        [InlineData("https://coronamelder-portal.nl/testendpoint")]
        [InlineData("https://acceptatie.coronamelder-portal.nl")]
        [InlineData("http://localhost:4200/")]
        [Theory]
        public void ShouldNotEndWithEndpoint(string testFrontendBaseUrl)
        {
            // Assemble
            var builder = new OriginBuilder(testFrontendBaseUrl);

            // Assert
            var testUri = new Uri(testFrontendBaseUrl);
            var origin = builder.GetOrigin();
            
            var result = origin.EndsWith(testUri.Host) || origin.EndsWith(testUri.Port.ToString());

            // Act
            Assert.True(result);
        }
    }
}