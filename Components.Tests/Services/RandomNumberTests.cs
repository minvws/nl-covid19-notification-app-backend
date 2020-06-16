using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass]
    public class RandomNumberTests
    {
        [TestMethod]
        public void Valid()
        {
            var random = new RandomNumberGenerator();
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
            var random = new RandomNumberGenerator();
            var result = random.GenerateToken(length);
            Assert.IsTrue(result.Length == length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Min value is higher than max value")]
        public void RangeTest()
        {
            var random = new RandomNumberGenerator();
            var minValue = 8;
            var maxValue = 5;
            random.Next(minValue, maxValue);
        }
    }
}
