// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass()]
    public class StandardUtcDateTimeProviderTests
    {
        [TestMethod()]
        public void NowTest()
        {
            Assert.IsTrue(new StandardUtcDateTimeProvider().Now().Kind == DateTimeKind.Utc);
        }
    }
}