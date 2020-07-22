// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

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
            var command = new HttpPostDecoyKeysCommand(new TestLogger<HttpPostDecoyKeysCommand>(), new TestRng(n), new DefaultDecoyKeysConfig());
            var timer = new Stopwatch();

            // Act
            timer.Start();
            IActionResult result = command.Execute().Result;
            timer.Stop();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(timer.ElapsedMilliseconds >= n);
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
