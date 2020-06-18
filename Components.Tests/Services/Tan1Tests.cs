// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass()]
    public class Tan1Tests
    {
        [TestMethod()]
        public void Examples()
        {
            var r = new Random();
            var buffer = new byte[16];
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            Trace.WriteLine(Convert.ToBase64String(buffer));
            r.NextBytes(buffer);
            var thing = Convert.ToBase64String(buffer);
            Trace.WriteLine(thing);
            var result = new Span<byte>(new byte[16]);
            Assert.IsTrue(Convert.TryFromBase64String(thing, result, out var length));
            Assert.AreEqual(16, length);
        }
    }
}