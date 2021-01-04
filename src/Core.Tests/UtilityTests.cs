// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class UtilityTests
    {
        [Fact]
        public void KeyGenerator()
        {
            Trace.WriteLine(DateTime.UtcNow.ToString("u"));


            var key = new byte[256 / 8];
            var r = new Random();
            for (var i = 0; i < 1000; i++)
            {
                r.NextBytes(key);
                var keyString = Convert.ToBase64String(key);
                Trace.WriteLine(string.Join(",", key.Select(x => x)));
                Trace.WriteLine(keyString);
                Trace.WriteLine(keyString.Length);
            }
        }
    }
}