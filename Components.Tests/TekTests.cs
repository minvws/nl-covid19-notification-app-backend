// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests
{


    [TestClass]
    public class TekValidatorTests
    {
        [DataRow(0, 0, 0, 0, true)]

        [DataRow(1, 0, 0, 0, false)]
        [DataRow(0, 0, 1, 0, false)]

        [DataRow(0, 1, 0, 1, true)]
        [DataRow(1, 0, 1, 0, true)]

        [DataRow(1000, 144, 1000, 144, true)]
        [DataRow(1000, 144, 1000, 143, false)]

        [DataTestMethod]
        public void StartHere(int leftStart, int leftPeriod, int rightStart, int rightPeriod, bool overlaps)
        {
            Assert.AreEqual(overlaps, new Tek { RollingStartNumber = leftStart, RollingPeriod = leftPeriod }
                .SameTime(new Tek { RollingStartNumber = rightStart, RollingPeriod = rightPeriod }));
        }
    }

}
