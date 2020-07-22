using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System.Diagnostics;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.DecoyKeys
{
    [TestClass]
    public class HttpPostDecoyKeyCommandTests
    {
        [DataRow(42)]
        [DataRow(64)]
        [DataRow(343)]
        [DataTestMethod]
        public void Execute_takes_at_least_N_milliseconds(int n)
        {
            // Assemble
            var config = new DefaultDecoyKeysConfig();
            var logger = new TestLogger<HttpPostDecoyKeysCommand>();
            var rng = new TestRng(n);
            var timer = new Stopwatch();
            var command = new HttpPostDecoyKeysCommand(logger, rng, config);

            // Act
            timer.Start();
            IActionResult result = command.Execute().Result;
            timer.Stop();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(timer.ElapsedMilliseconds >= rng.Next(0, 0));
        }

        private class TestRng : IRandomNumberGenerator
        {
            private readonly int _Result;
            public TestRng(int result) => _Result = result;
            public string GenerateToken(int length = 6) => "tester";
            public int Next(int min, int max) => _Result;
            public byte[] GenerateKey(int keyLength = 32) => new byte[keyLength];
        }
    }
}
