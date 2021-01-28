// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.DecoyKeys
{
    public class WelfordsAlgorithmTests
    {
        [Fact]
        public void NoRegistrations()
        {
            //Arrange
            var wa = new WelfordsAlgorithm();

            //Assert
            Assert.Equal(0, wa.GetNormalSample(), 2);
        }

        [Fact]
        public void NegativeAmount()
        {
            //Arrange
            var wa = new WelfordsAlgorithm();
            wa.AddDataPoint(-1);
            //Assert
            Assert.Equal(-1, wa.GetNormalSample());
        }

        [Fact]
        public void ZeroAmount()
        {
            //Arrange
            var wa = new WelfordsAlgorithm();
            wa.AddDataPoint(0);
            //Assert
            Assert.Equal(0, wa.GetNormalSample(), 0);
        }

        [Theory]
        [ClassData(typeof(DecoyTimeTestData))]
        public void CalculateMeanAndStdDev(double[] inputValues, double expectedMean, double expectedStDev)
        {
            //Arrange
            var sut = new WelfordsAlgorithm();
            WelfordsAlgorithmState result = null;

            //Act
            foreach (var dataPoint in inputValues)
            {
                result = sut.AddDataPoint(dataPoint);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMean, result.Mean, 2);
            Assert.Equal(expectedStDev, result.StandardDeviation, 2);
        }

        private class DecoyTimeTestData : IEnumerable<object[]>
        {
            //ncrunch: no coverage start
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { new double[] { 10.0 }, 10.0, 0 };
                yield return new object[] { new double[] { 10.0, 10.0 }, 10.0, 0 };
                yield return new object[] { new double[] { 10.0, 9.0, 11.0 }, 10.0, 1.0 };
                yield return new object[] { new double[] { 2, 4, 6, 8, 10, 12, 14, 17, 20 }, 10.33, 6 };
                yield return new object[] { new double[] { 3.5, 7.2, 1.1, 0.2 }, 3, 3.1273 };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            //ncrunch: no coverage end
        }
    }
}
