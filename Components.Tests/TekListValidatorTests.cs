// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests
{
    //[TestClass]
    //public class TekListValidatorTests
    //{
    //    [TestMethod]
    //    public void TeksEmpty()
    //    {
    //        Assert.AreEqual(false, new TekListOverlapValidator().Validate(new Tek[0]).Length > 0);
    //    }

    //    [TestMethod]
    //    public void Teks1()
    //    {
    //        Assert.AreEqual(false, new TekListOverlapValidator().Validate(new[] { new Tek { RollingPeriod = 0, RollingStartNumber = 0} }).Length > 0);
    //    }

    //    [DataRow(0, 0, 0, 0, true)]

    //    [DataRow(1, 0, 0, 0, false)]
    //    [DataRow(0, 0, 1, 0, false)]

    //    [DataRow(0, 1, 0, 0, true)]
    //    [DataRow(0, 0, 0, 1, true)]

    //    [DataRow(1000, 144, 1144, 144, true)]
    //    [DataRow(1000, 144, 1145, 144, false)]

    //    [DataRow(1144, 144, 1000, 144, true)]
    //    [DataRow(1145, 144, 1000, 144, false)]

    //    [DataTestMethod]
    //    public void Teks2(int leftStart, int leftPeriod, int rightStart, int rightPeriod, bool overlaps)
    //    {

    //        var teks = new[] { new Tek { RollingStartNumber = leftStart, RollingPeriod = leftPeriod, KeyData = new byte[0] }, new Tek { RollingStartNumber = rightStart, RollingPeriod = rightPeriod, KeyData = new byte[] { 1 } } };

    //        Assert.AreEqual(overlaps, new TekListOverlapValidator().Validate(teks).Length > 0);
    //    }

        
    //    [DataRow(0, 0, 0, 0, 0, 0, true)]

    //    [DataRow(1, 0, 0, 0, 2, 0, false)]
    //    [DataRow(2, 0, 1, 0, 0, 0, false)]

    //    [DataRow(0, 1, 0, 0, 2, 0, true)]
    //    [DataRow(0, 0, 0, 1, 2, 0, true)]

    //    [DataRow(1000, 144, 1144, 144, 2000, 144, true)]
    //    [DataRow(1000, 144, 1145, 144, 2000, 144, false)]

    //    [DataRow(1144, 144, 1000, 144, 2000, 144, true)]
    //    [DataRow(1145, 144, 1000, 144, 2000, 144, false)]

    //    [DataTestMethod]
    //    public void Teks2(int leftStart, int leftPeriod, int rightStart, int rightPeriod, int start2, int period2, bool overlaps)
    //    {

    //        var teks = new[] { 
    //            new Tek { RollingStartNumber = leftStart, RollingPeriod = leftPeriod, KeyData = new byte[0] },
    //            new Tek { RollingStartNumber = rightStart, RollingPeriod = rightPeriod, KeyData = new byte[] { 1 } },
    //            new Tek { RollingStartNumber = start2, RollingPeriod = period2, KeyData = new byte[] { 2 } }
    //        };

    //        Assert.AreEqual(overlaps, new TekListOverlapValidator().Validate(teks).Length > 0);
    //    }
    //}
}