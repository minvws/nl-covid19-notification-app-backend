// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [TestClass]
    public class EksBatchJobMk3Tests
    {
        private Func<WorkflowDbContext> _WorkflowFac;
        private Func<PublishingJobDbContext> _PublishingFac;
        private Func<ContentDbContext> _ContentFac;

        private LoggerFactory _Lf;
        private FakeEksConfig _FakeEksConfig;
        private ExposureKeySetBatchJobMk3 _Engine;
        private DateTime _Now;

        private class FakeEksConfig : IEksConfig
        {
            public int LifetimeDays { get; set; } = 14;

            public int TekCountMax { get; set; } = 20;

            public int TekCountMin { get; set; } = 10;

            public int PageSize { get; set; } = 100;
        }

        private class FakeEksBuilder : IEksBuilder
        {
            public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
            {
                return new byte[] {1};
            }
        }

        private class FakeTekValidatorConfig : ITekValidatorConfig
        {
            public int MaxAgeDays { get; set; } = 14;
            public int KeyDataLength { get; set; } = 16;
            public int RollingPeriodMin { get; set; } = 0;
            public int RollingPeriodMax { get; set; } = 144;
            public int RollingStartNumberMin { get; set; } = 10000;
            public int PublishingDelayInMinutes => throw new NotImplementedException(); //ncrunch: no coverage
            public int AuthorisationWindowMinutes => throw new NotImplementedException(); //ncrunch: no coverage
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FireTwice()
        {
            EksEngineResult();
            _Engine.Execute().GetAwaiter().GetResult();
        } //ncrunch: no coverage


        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            var db = _WorkflowFac();
            db.KeyReleaseWorkflowStates.AddRange(workflows);
            db.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            db.SaveChanges();

            Assert.AreEqual(workflows.Length, db.KeyReleaseWorkflowStates.Count());
            Assert.AreEqual(workflows.Sum(x => x.Teks.Count) , db.TemporaryExposureKeys.Count());
        }

        private static TekEntity CreateTek(int rsn)
        {
            return new TekEntity { RollingStartNumber = rsn, RollingPeriod = 2, KeyData = new byte[16], Region = "NL", PublishAfter = DateTime.UtcNow.AddHours(-1)};
        }

        private static TekReleaseWorkflowStateEntity Create(DateTime now, params TekEntity[] arse)
        {
            return new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[0],
                ConfirmationKey = new byte[0],
                AuthorisedByCaregiver = now.AddHours(1),
                Created = now,
                ValidUntil = now.AddDays(1),
                DateOfSymptomsOnset = now.AddDays(-1).Date,
                Teks = arse
            };
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Teks1_NoRiskNotStuffed()
        {
            //One TEK from the dawn of time.
            var wfs = new[]
            {
                Create(_Now, CreateTek(1))
            };

            Write(wfs);

            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(1, result.InputCount);
            Assert.AreEqual(0, result.StuffingCount);
            Assert.AreEqual(0, result.OutputCount);
            Assert.AreEqual(1, result.TransmissionRiskNoneCount);
            Assert.AreEqual(0, result.EksInfo.Length);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Teks0_NothingToSeeHereMoveAlong()
        {
            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(0, result.InputCount);
            Assert.AreEqual(0, result.StuffingCount);
            Assert.AreEqual(0, result.OutputCount);
            Assert.AreEqual(0, result.EksInfo.Length);
            Assert.AreEqual(0, result.TransmissionRiskNoneCount);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Teks1_GetStuffed()
        {
            var wfs = new[]
            {
                Create(_Now, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1,0,0,0,DateTimeKind.Utc));
            Assert.AreEqual(1, result.InputCount);
            Assert.AreEqual(4, result.StuffingCount);
            Assert.AreEqual(5, result.OutputCount);
            Assert.AreEqual(1, result.EksInfo.Length);
            Assert.AreEqual(5, result.EksInfo[0].TekCount);
            Assert.AreEqual(0, result.TransmissionRiskNoneCount);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Tek5_NotStuffed()
        {

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_Now, teks)
            };

            Write(wfs);

            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(5, result.InputCount);
            Assert.AreEqual(0, result.StuffingCount);
            Assert.AreEqual(5, result.OutputCount);
            Assert.AreEqual(1, result.EksInfo.Length);
            Assert.AreEqual(5, result.EksInfo[0].TekCount);
            Assert.AreEqual(0, result.TransmissionRiskNoneCount);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Tek10_NotStuffed()
        {
            var teks = Enumerable.Range(1, 10)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_Now, teks)
            };

            Write(wfs);
            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(10, result.InputCount);
            Assert.AreEqual(0, result.StuffingCount);
            Assert.AreEqual(10, result.OutputCount);
            Assert.AreEqual(1, result.EksInfo.Length);
            Assert.AreEqual(10, result.EksInfo[0].TekCount);
            Assert.AreEqual(0, result.TransmissionRiskNoneCount);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        [TestMethod]
        [ExclusivelyUses("DbTest1")]
        public void Tek11_NotStuffed_2Eks()
        {
            var teks = Enumerable.Range(1, 11)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_Now, teks)
            };

            Write(wfs);

            var result = EksEngineResult();
            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(11, result.InputCount);
            Assert.AreEqual(0, result.StuffingCount);
            Assert.AreEqual(11, result.OutputCount);
            Assert.AreEqual(2, result.EksInfo.Length);
            Assert.AreEqual(10, result.EksInfo[0].TekCount);
            Assert.AreEqual(1, result.EksInfo[1].TekCount);
            Assert.AreEqual(0, result.TransmissionRiskNoneCount);

            Assert.AreEqual(0, result.ReconcileOutputCount);
            Assert.AreEqual(0, result.ReconcileEksSumCount);

            Assert.AreEqual(_ContentFac().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.AreEqual(_WorkflowFac().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.IsTrue(result.TotalSeconds > 0);
        }

        private EksEngineResult EksEngineResult()
        {
            _Engine = new ExposureKeySetBatchJobMk3(
                _FakeEksConfig,
                new FakeEksBuilder(),
                _PublishingFac,
                new StandardUtcDateTimeProvider(),
                _Lf.CreateLogger<ExposureKeySetBatchJobMk3>(),
                new EksStuffingGenerator(new StandardRandomNumberGenerator(), new FakeTekValidatorConfig()),
                new SnapshotEksInputMk1(_Lf.CreateLogger<SnapshotEksInputMk1>(), new TransmissionRiskLevelCalculationV1(), new StandardUtcDateTimeProvider(), _WorkflowFac(), _PublishingFac),
                new MarkWorkFlowTeksAsUsed(_WorkflowFac, _FakeEksConfig, _PublishingFac, _Lf.CreateLogger<MarkWorkFlowTeksAsUsed>()),
                new EksJobContentWriter(_ContentFac, _PublishingFac, new Sha256HexPublishingIdService(), _Lf.CreateLogger<EksJobContentWriter>())
                );

            return _Engine.Execute().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void SetupSql()
        {
            _Now = DateTime.UtcNow;
            
            _WorkflowFac = () => new WorkflowDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Initial Catalog=WorkflowTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
                .Options);

            var wf = _WorkflowFac();
            wf.Database.EnsureDeleted();
            wf.Database.EnsureCreated();

            _PublishingFac = () => new PublishingJobDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Initial Catalog=PublishingTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
                .Options);

            var pdb = _PublishingFac();
            pdb.Database.EnsureDeleted();
            pdb.Database.EnsureCreated();

            _ContentFac = () => new ContentDbContext(new DbContextOptionsBuilder()
                .UseSqlServer("Initial Catalog=ContentTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
                .Options);

            var cdb = _ContentFac();
            cdb.Database.EnsureDeleted();
            cdb.Database.EnsureCreated();

            _FakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };

            _Lf = new LoggerFactory();
        }
    }
}