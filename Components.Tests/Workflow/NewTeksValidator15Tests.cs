// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks.Tests
//{
//    [TestClass()]
//    public class NewTeksValidator15Tests
//    {
//        private class FakedNow : IUtcDateTimeProvider
//        {
//            private readonly DateTime _Fixed;

//            public FakedNow(DateTime @fixed)
//            {
//                _Fixed = @fixed;
//            }

//            public DateTime Now() => _Fixed;
//        }

//        Tek GenerateTek(int m, int d, int q)
//        {
//            var keyData = BitConverter.GetBytes(d * 100 + q);
//            var t = new DateTime(2020, m, d, 0, 0, 1).ToRollingPeriodStart();
//            return new Tek {RollingStartNumber = t, KeyData = keyData, RollingPeriod = 144};
//        }



//        [TestMethod()]
//        public void Gaen15SameDayTekReleaseOn()
//        {
//            var keysOnDevice = Enumerable.Range(1, 14).Select(x => GenerateTek(9, x, 1)).ToList();

//            var workflow1 = new TekReleaseWorkflowStateEntity
//            { 
//                Created = new DateTime(2020, 9, 14, 10, 0, 0),
//                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
//            };

//            var r0 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 14, 9, 35, 0))).Validate(keysOnDevice.ToArray(), workflow1);
//            Assert.AreEqual(0, r0.Length);

//            foreach (var i in keysOnDevice)
//                workflow1.Keys.Add(Mapper.MapToEntity(i)); //These will be lost cos they don't get GGD authorisation

//            keysOnDevice.Add(GenerateTek(9, 14, 2));

//            Assert.AreEqual(15, keysOnDevice.Count);

//            //Sep 15
//            keysOnDevice.RemoveAt(0);
//            keysOnDevice.Add(GenerateTek(9, 15, 1));

//            var workflow2 = new TekReleaseWorkflowStateEntity
//            {
//                Created = new DateTime(2020, 9, 14, 10, 0, 0),
//                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
//            };

//            keysOnDevice.Add(GenerateTek(9, 15, 2));

//            workflow2.LabConfirmation = new DateTime(2020, 9, 15, 11, 0, 0);
//            var r1 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 15, 11, 5, 0))).Validate(keysOnDevice.ToArray(), workflow2);
//            Assert.AreEqual(0, r1.Length);

//            //TODO update workflow2 keys

//            keysOnDevice.Add(GenerateTek(9, 15, 3));

//            //11:20 Server publishes Server publishes K0902.1 through K0915.2 to the CDN.

//            var r2 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 15, 14, 0, 0))).Validate(keysOnDevice.ToArray(), workflow2);
//            Assert.AreEqual(0, r2.Length); //14.00h Server silently discards all keys as they arrive > 120 minutes after GGD code

//            keysOnDevice.Add(GenerateTek(9, 15, 4));
//            Assert.AreEqual(18, keysOnDevice.Count);

//            //Sep 16
//            keysOnDevice.RemoveAt(0);
//            keysOnDevice.Add(GenerateTek(9, 16, 1));

//            var r3 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 16, 0, 30, 0))).Validate(keysOnDevice.ToArray(), workflow2);
//            //workflow2:
//            // - ignores the keys it already has
//            // - K0916.1 is discarded because it's a key for today and the bucket doesn't accept same day keys after midnight.
//            Assert.AreEqual(0, r3.Length); 
//        }

//        [TestMethod()]
//        public void Gaen14or15SameDayTekReleaseOff()
//        {
//            var keysOnDevice = Enumerable.Range(1, 14).Select(x => GenerateTek(9, x, 1)).ToList();

//            var workflow1 = new TekReleaseWorkflowStateEntity
//            {
//                Created = new DateTime(2020, 9, 14, 10, 0, 0),
//                ValidUntil = new DateTime(2020, 9, 15, 4, 0, 0),
//            };

//            var r0 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 14, 10, 0, 0))).Validate(keysOnDevice.ToArray(), workflow1);
//            Assert.AreEqual(0, r0.Length);

//            foreach (var i in keysOnDevice)
//                workflow1.Keys.Add(Mapper.MapToEntity(i)); //These will be lost cos they don't get GGD authorisation

//            Assert.AreEqual(14, keysOnDevice.Count);

//            //Sep 15
//            keysOnDevice.RemoveAt(0);
//            var last = keysOnDevice.Last();
//            keysOnDevice.Add(GenerateTek(9, 15, 1));

//            //Sep 15 00:30
//            var r1 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 15, 11, 5, 0))).Validate(new[] { last}, workflow1);
//            Assert.AreEqual(0, r1.Length);

//            //Sep 15 10:00
//            var workflow2 = new TekReleaseWorkflowStateEntity
//            {
//                Created = new DateTime(2020, 9, 15, 10, 0, 0),
//                ValidUntil = new DateTime(2020, 9, 16, 4, 0, 0),
//            };

//            //Sep 15 11:00
//            workflow2.LabConfirmation = new DateTime(2020, 9, 15, 11, 0, 0);
//            var r2 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 15, 11, 5, 0))).Validate(keysOnDevice.Take(keysOnDevice.Count-1).ToArray(), workflow2);
//            Assert.AreEqual(0, r2.Length);

//            //Sep 15 11:20 Server publishes K0902.1 through K0915.2 to the CDN.
//            var r3 = new NewTeksValidator15(new FakedNow(new DateTime(2020, 9, 15, 12, 0, 0))).Validate(keysOnDevice.Take(keysOnDevice.Count - 1).ToArray(), workflow2);
//            Assert.AreEqual(0, r3.Length);

//            Assert.AreEqual(14, keysOnDevice.Count);

//            //Sep 16 11:00
//            keysOnDevice.RemoveAt(0);
//            keysOnDevice.Add(GenerateTek(9, 15, 1));
//            //The nightly batch uploads all keys, including now K0915.1 to bucket B ???
         
//        }
//    }
//}