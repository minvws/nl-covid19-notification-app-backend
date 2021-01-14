// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Collections;
using Xunit;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.DecoyTime
{
	public class DecoyTimeCalculatorTest
	{
		private DecoyKeysLoggingExtensions _Logger;

		public DecoyTimeCalculatorTest()
		{
			var loggerfactory = new LoggerFactory();

			_Logger = new DecoyKeysLoggingExtensions(loggerfactory.CreateLogger<DecoyKeysLoggingExtensions>());
		}

		[Theory]
		[ClassData(typeof(DecoyTimeTestData))]
		public void RegisterTime_CalculateMeanAndStdDev(double[] inputValues, double expectedMean, double expectedStDev)
		{
			//Arrange
			var sut = new DecoyTimeCalculator(_Logger);

			//Act
			foreach (var dataPoint in inputValues)
			{
				sut.RegisterTime(dataPoint);
			}

			var resultMean = sut.DecoyTimeMean;
			var resultStDev = sut.DecoyTimeStDev;

			//Assert
			Assert.Equal(expectedMean, resultMean, 2);
			Assert.Equal(expectedStDev, resultStDev, 2);
		}

		[Theory]
		[InlineData(0.0)]
		[InlineData(-12.4)]
		public void RegisterTime_ThrowsOnNonPositiveInput(double incorrectTime)
		{
			//Arrange
			var sut = new DecoyTimeCalculator(_Logger);
			Action runSut = () => sut.RegisterTime(incorrectTime);

			//Assert
			Assert.Throws<ArgumentOutOfRangeException>(runSut);
		}

		private class DecoyTimeTestData : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { new double[] { }, 0, 0 };
				yield return new object[] { new double[] { 10.0 }, 10.0, 0 };
				yield return new object[] { new double[] { 10.0, 10.0 }, 10.0, 0 };
				yield return new object[] { new double[] { 10.0, 9.0, 11.0 }, 10.0, 1.0 };
				yield return new object[] { new double[] { 2, 4, 6, 8, 10, 12, 14, 17, 20 }, 10.33, 6 };
				yield return new object[] { new double[] { 3.5, 7.2, 1.1, 0.2 }, 3, 3.1273 };
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
