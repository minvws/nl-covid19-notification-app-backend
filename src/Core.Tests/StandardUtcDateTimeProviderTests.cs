// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class StandardUtcDateTimeProviderTests
    {
        [Fact]
        public void NowKind()
        {
            Assert.True(new StandardUtcDateTimeProvider().Now().Kind == DateTimeKind.Utc);
        }

        [Fact]
        public void SnapshotKind()
        {
            Assert.True(new StandardUtcDateTimeProvider().Snapshot.Kind == DateTimeKind.Utc);
        }

        [Fact]
        public void SnapshotSame()
        {
            var dtp = new StandardUtcDateTimeProvider();
            var v0 = dtp.Snapshot;
            Thread.Sleep(500);
            var v1 = dtp.Snapshot;
            Assert.Equal(v0,v1);
        }
    }
}