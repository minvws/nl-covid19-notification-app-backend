// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Components.Tests.Commands
//{
//    [TestClass]
//    public class ExposureKeySetLifecycleTests
//    {
//        [TestMethod]
//        public void StartHere()
//        {
//            var cuckooFilter = "Cuckoo!Cuckoo!Cuckoo!Cuckoo!Cuckoo!Sparrow!";

//            var localTestConfigProvider = new McsTestConfigProvider();
//            var standardUtcDateTimeProvider = new StandardUtcDateTimeProvider();

//            var ts1 = new ExposureKeySetCreateCommand(localTestConfigProvider, standardUtcDateTimeProvider).Execute(cuckooFilter);
//            var ts2 = new ExposureKeySetCreateCommand(localTestConfigProvider, standardUtcDateTimeProvider).Execute(cuckooFilter);
//            Assert.AreEqual(ts2.Result.ExposureKeySetId, ts1.Result.ExposureKeySetId + 1);

//            var latest = new ExposureKeySetGetLastIdCommand(localTestConfigProvider).Execute();
//            Assert.AreEqual(ts2.Result.ExposureKeySetId, latest);
//        }
//    }
//}