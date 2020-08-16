// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass]
    public class StandardUtcDateTimeProviderTests
    {
        [TestMethod]
        public void NowKind()
        {
            Assert.IsTrue(new StandardUtcDateTimeProvider().Now().Kind == DateTimeKind.Utc);
        }

        [TestMethod]
        public void SnapshotKind()
        {
            Assert.IsTrue(new StandardUtcDateTimeProvider().Snapshot.Kind == DateTimeKind.Utc);
        }

        [TestMethod]
        public void SnapshotSame()
        {
            var dtp = new StandardUtcDateTimeProvider();
            var v0 = dtp.Snapshot;
            Thread.Sleep(500);
            var v1 = dtp.Snapshot;
            Assert.AreEqual(v0,v1);
        }
    }
}