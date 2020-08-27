// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System;
using System.Diagnostics;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.DecoyKeys
{
    public class HttpPostDecoyKeyCommandTests
    {
        [Theory]
        [InlineData(42)]
        [InlineData(64)]
        [InlineData(343)]
        public void Execute_takes_at_least_N_milliseconds(int n)
        {
            // Assemble
            var command = new HttpPostDecoyKeysCommand(new TestLogger<HttpPostDecoyKeysCommand>(), new TestRng(n), new DefaultDecoyKeysConfig());
            var timer = new Stopwatch();

            // Act
            timer.Start();
            var result = command.Execute().Result;
            timer.Stop();

            // Assert
            Assert.NotNull(result);
            Assert.True(timer.ElapsedMilliseconds >= n);
        }

        private class TestRng : IRandomNumberGenerator
        {
            private readonly int _Result;
            public TestRng(int result) => _Result = result;
            public int Next(int min, int max) => _Result;
            //ncrunch: no coverage start
            public byte[] NextByteArray(int _)
            {
                throw new NotImplementedException();
            }
            //ncrunch: no coverage end
        }
    }
}
