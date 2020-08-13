//// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
//// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
//// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using NCrunch.Framework;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
//{
//    [TestClass]
//    public class EksBatchJobTests
//    {
//        private SqliteConnection _ConnectionPub;
//        private SqliteConnection _ConnectionCon;
//        private SqliteConnection _ConnectionWf;
//        private LoggerFactory _Lf;

//        private Func<WorkflowDbContext> _WorkflowFac;
//        private Func<WorkflowDbContext> _WorkflowFac;
//        private ContentDbContext _Content;
//        private FakeEksConfig _FakeEksConfig;
//        private FakeSnapshotter _FakeSnapshotter;
//        private ExposureKeySetBatchJobMk3 _Engine;

//        private class FakeEksConfig : IEksConfig
//        {
//            public int LifetimeDays { get; set; } = 14;

//            public int TekCountMax { get; set; } = 20;

//            public int TekCountMin { get; set; } = 10;

//            public int PageSize { get; set; } = 100;
//        }

//        private class FakeEksBuilder : IEksBuilder
//        {
//            public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
//            {
//                return new byte[] {1};
//            }
//        }

//        private class FakeTekValidatorConfig : ITekValidatorConfig
//        {
//            public int MaxAgeDays { get; set; } = 14;
//            public int KeyDataLength { get; set; } = 16;
//            public int RollingPeriodMin { get; set; } = 0;
//            public int RollingPeriodMax { get; set; } = 144;
//            public int RollingStartNumberMin { get; set; } = 10000;
//            public int PublishingDelayInMinutes => throw new NotImplementedException(); //ncrunch: no coverage
//            public int AuthorisationWindowMinutes => throw new NotImplementedException(); //ncrunch: no coverage
//        }

//        private class FakeSnapshotter : ISnapshotEksInput
//        {
//            private readonly PublishingJobDbContext _PublishingJobDbContext;
//            public EksCreateJobInputEntity[] Items { get; set; } = new EksCreateJobInputEntity[0];

//            public FakeSnapshotter(PublishingJobDbContext publishingJobDbContext)
//            {
//                _PublishingJobDbContext = publishingJobDbContext ?? throw new ArgumentNullException(nameof(publishingJobDbContext));
//            }

//            public async Task<SnapshotEksInputResult> Execute(DateTime snapshotStart)
//            {
//                _PublishingJobDbContext.EksInput.AddRange(Items);
//                _PublishingJobDbContext.SaveChanges();
//                return new SnapshotEksInputResult {SnapshotSeconds = 123, TekInputCount = Items.Length };
//            }
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        [ExpectedException(typeof(InvalidOperationException))]
//        public void FireTwice()
//        {
//            EksEngineResult();
//            _Engine.Execute().GetAwaiter().GetResult();
//        } //ncrunch: no coverage

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Teks1_NoRiskNotStuffed()
//        {
//            _FakeSnapshotter.Items = new[]
//            {
//                new EksCreateJobInputEntity { RollingStartNumber = 1, RollingPeriod = 2, TransmissionRiskLevel = TransmissionRiskLevel.None, KeyData = new byte[16], TekId = 1}
//            };
//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(1, result.TekInputCount);
//            Assert.AreEqual(0, result.TekStuffingCount);
//            Assert.AreEqual(0, result.TekTotalCount);
//            Assert.AreEqual(0, result.EksInfo.Length);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Teks0_NothingToSeeHereMoveAlong()
//        {
//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(0, result.TekInputCount);
//            Assert.AreEqual(0, result.TekStuffingCount);
//            Assert.AreEqual(0, result.TekTotalCount);
//            Assert.AreEqual(0, result.EksInfo.Length);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Teks1_GetStuffed()
//        {
//            _FakeSnapshotter.Items = new[]
//            {
//                new EksCreateJobInputEntity { RollingStartNumber = 1, RollingPeriod = 2, TransmissionRiskLevel = TransmissionRiskLevel.High, KeyData = new byte[16], TekId = 1}
//            };

//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(1, result.TekInputCount);
//            Assert.AreEqual(4, result.TekStuffingCount);
//            Assert.AreEqual(5, result.TekTotalCount);
//            Assert.AreEqual(1, result.EksInfo.Length);
//            Assert.AreEqual(5, result.EksInfo[0].TekCount);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Tek5_NotStuffed()
//        {
//            _FakeSnapshotter.Items = Enumerable.Range(1, 5)
//                .Select(x => new EksCreateJobInputEntity { RollingStartNumber = x, RollingPeriod = 2, TransmissionRiskLevel = TransmissionRiskLevel.High, KeyData = new byte[16], TekId = 1 })
//                .ToArray();

//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(5, result.TekInputCount);
//            Assert.AreEqual(0, result.TekStuffingCount);
//            Assert.AreEqual(5, result.TekTotalCount);
//            Assert.AreEqual(1, result.EksInfo.Length);
//            Assert.AreEqual(5, result.EksInfo[0].TekCount);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Tek10_NotStuffed()
//        {
//            _FakeSnapshotter.Items = Enumerable.Range(1, 10)
//                .Select(x => new EksCreateJobInputEntity { RollingStartNumber = x, RollingPeriod = 2, TransmissionRiskLevel = TransmissionRiskLevel.High, KeyData = new byte[16], TekId = 1 })
//                .ToArray();

//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(10, result.TekInputCount);
//            Assert.AreEqual(0, result.TekStuffingCount);
//            Assert.AreEqual(10, result.TekTotalCount);
//            Assert.AreEqual(1, result.EksInfo.Length);
//            Assert.AreEqual(10, result.EksInfo[0].TekCount);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        [TestMethod]
//        [ExclusivelyUses("LocalDb")]
//        public void Tek11_NotStuffed_2Eks()
//        {
//            _FakeSnapshotter.Items = Enumerable.Range(1, 11)
//                .Select(x => new EksCreateJobInputEntity {RollingStartNumber = x, RollingPeriod = 2, TransmissionRiskLevel = TransmissionRiskLevel.High, KeyData = new byte[16], TekId = 1})
//                .ToArray();

//            var result = EksEngineResult();
//            Assert.IsTrue(result.Started > new DateTime(2020, 8, 1));
//            Assert.AreEqual(11, result.TekInputCount);
//            Assert.AreEqual(0, result.TekStuffingCount);
//            Assert.AreEqual(11, result.TekTotalCount);
//            Assert.AreEqual(2, result.EksInfo.Length);
//            Assert.AreEqual(10, result.EksInfo[0].TekCount);
//            Assert.AreEqual(1, result.EksInfo[1].TekCount);
//            Assert.IsTrue(result.TotalSeconds > 0);
//        }

//        private PublishingJobDbContext GetPublishingJobDbContext() new PublishingJobDbContext(new DbContextOptions<>()

//        private EksEngineResult EksEngineResult()
//        {
//            _Engine = new ExposureKeySetBatchJobMk3(
//                _FakeEksConfig,
//                new FakeEksBuilder(),
//                GetPublishingJobDbContext,
//                new StandardUtcDateTimeProvider(),
//                _Lf.CreateLogger<ExposureKeySetBatchJobMk3>(),
//                new EksStuffingGenerator(new StandardRandomNumberGenerator(), new FakeTekValidatorConfig()),
//                _FakeSnapshotter,
//                new MarkWorkFlowTeksAsUsed(_WorkflowFac, _FakeEksConfig, _Lf.CreateLogger<MarkWorkFlowTeksAsUsed>()),
//            );

//            return _Engine.Execute().GetAwaiter().GetResult();
//        }

//        [TestInitialize]
//        public void Setup()
//        {
//            _ConnectionPub = new SqliteConnection("Data Source=:memory:");
//            _PublishingDbContext = new PublishingJobDbContext(new DbContextOptionsBuilder().UseSqlite(_ConnectionPub).Options);
//            _ConnectionPub.Open();
//            _PublishingDbContext.Database.EnsureCreated();

//            _ConnectionCon = new SqliteConnection("Data Source=:memory:");
//            _Content = new ContentDbContext(new DbContextOptionsBuilder().UseSqlite(_ConnectionCon).Options);
//            _ConnectionCon.Open();
//            _Content.Database.EnsureCreated();

//            _ConnectionWf = new SqliteConnection("Data Source=:memory:");
//            _WorkflowFac = () => new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlite(_ConnectionWf).Options);
//            _ConnectionWf.Open();
//            _WorkflowFac().Database.EnsureCreated();

//            _FakeSnapshotter = new FakeSnapshotter(_PublishingDbContext);

//            _FakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };

//            _Lf = new LoggerFactory();
//        }
//    }
//}