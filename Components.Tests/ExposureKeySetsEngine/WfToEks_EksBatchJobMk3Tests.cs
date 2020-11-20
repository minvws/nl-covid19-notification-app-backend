// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    /// <summary>
    /// NB Change to use a DK source rather than WFs
    /// </summary>
    public abstract class WfToEks_EksBatchJobMk3Tests : IDisposable
    {
        #region Implementation

        private readonly FakeEksConfig _FakeEksConfig;
        private readonly IDbProvider<WorkflowDbContext> _WorkflowFac;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _EksPublishingJobDbProvider;
        private readonly IDbProvider<ContentDbContext> _ContentDbProvider;
        private readonly IWrappedEfExtensions _EfExtensions;

        private readonly LoggerFactory _Lf;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        private ExposureKeySetBatchJobMk3 _Engine;
        private readonly SnapshotWorkflowTeksToDksCommand _Snapshot;

        protected WfToEks_EksBatchJobMk3Tests(IDbProvider<WorkflowDbContext> workflowFac, IDbProvider<DkSourceDbContext> dkSourceFac, IDbProvider<EksPublishingJobDbContext> publishingFac, IDbProvider<ContentDbContext> contentFac, IWrappedEfExtensions efExtensions)
        {
            _WorkflowFac = workflowFac ?? throw new ArgumentNullException(nameof(workflowFac));
            _DkSourceDbProvider = dkSourceFac ?? throw new ArgumentNullException(nameof(dkSourceFac));
            _EksPublishingJobDbProvider = publishingFac ?? throw new ArgumentNullException(nameof(publishingFac));
            _ContentDbProvider = contentFac ?? throw new ArgumentNullException(nameof(contentFac));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));

            _DateTimeProvider = new StandardUtcDateTimeProvider();
            _FakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };
            _Lf = new LoggerFactory();

            _Snapshot = new SnapshotWorkflowTeksToDksCommand(_Lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                new StandardUtcDateTimeProvider(),
                new TransmissionRiskLevelCalculationMk2(),
                _WorkflowFac.CreateNew(),
                _WorkflowFac.CreateNew,
                _DkSourceDbProvider.CreateNew, 
                _EfExtensions,
                new IDiagnosticKeyProcessor[0]
                );
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            var db = _WorkflowFac.CreateNew();
            db.KeyReleaseWorkflowStates.AddRange(workflows);
            db.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            db.SaveChanges();

            Assert.Equal(workflows.Length, db.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count) , db.TemporaryExposureKeys.Count());

            _Snapshot.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static TekEntity CreateTek(int rsn)
        {
            return new TekEntity { RollingStartNumber = rsn, RollingPeriod = 2, KeyData = new byte[16], PublishAfter = DateTime.UtcNow.AddHours(-1)};
        }

        private static TekReleaseWorkflowStateEntity Create(DateTime now, params TekEntity[] items)
        {
            return new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[0],
                ConfirmationKey = new byte[0],
                AuthorisedByCaregiver = now.AddHours(1),
                Created = now,
                ValidUntil = now.AddDays(1),
                DateOfSymptomsOnset = now.AddDays(-1).Date,
                Teks = items
            };
        }

        private EksEngineResult RunEngine()
        {
            _Engine = new ExposureKeySetBatchJobMk3(
                _FakeEksConfig,
                new FakeEksBuilder(),
                _EksPublishingJobDbProvider.CreateNew,
                new StandardUtcDateTimeProvider(),
                new EksEngineLoggingExtensions(_Lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), new StandardRandomNumberGenerator(), _DateTimeProvider, _FakeEksConfig),
                new SnapshotDiagnosisKeys(_Lf.CreateLogger<SnapshotDiagnosisKeys>(), _DkSourceDbProvider.CreateNew(), _EksPublishingJobDbProvider.CreateNew),
                new MarkDiagnosisKeysAsUsedLocally(_DkSourceDbProvider.CreateNew, _FakeEksConfig, _EksPublishingJobDbProvider.CreateNew, _Lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_ContentDbProvider.CreateNew, _EksPublishingJobDbProvider.CreateNew, new Sha256HexPublishingIdService(), 
                    new EksJobContentWriterLoggingExtensions(_Lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_DkSourceDbProvider.CreateNew(), _EksPublishingJobDbProvider.CreateNew()),
                _EfExtensions
                );

            return _Engine.ExecuteAsync().GetAwaiter().GetResult();
        }

        private class FakeEksConfig : IEksConfig
        {
            public int LifetimeDays { get; set; } = 14;
            public int TekCountMax { get; set; } = 20;
            public int TekCountMin { get; set; } = 10;
            public int PageSize { get; set; } = 100;
            public bool CleanupDeletesData => throw new NotImplementedException(); //ncrunch: no coverage
        }

        private class FakeEksBuilder : IEksBuilder
        {
            public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys) => new byte[] { 1 };
        }

        public void Dispose()
        {
            _WorkflowFac.Dispose();
            _DkSourceDbProvider.Dispose();
            _EksPublishingJobDbProvider.Dispose();
            _ContentDbProvider.Dispose();
            _Lf.Dispose();
        }

        #endregion

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void FireSameEngineTwice()
        {
            RunEngine();
            Assert.Throws<InvalidOperationException>(() => _Engine.ExecuteAsync().GetAwaiter().GetResult());
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void FireTwice()
        {
            //One TEK from the dawn of time.
            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, CreateTek(1))
            };

            Write(wfs);

            var result = RunEngine();
            var firstInputCount = result.InputCount;
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(1, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);

            result = RunEngine();

            //Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), firstInputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Teks1_NoRiskNotStuffed()
        {
            //One TEK from the dawn of time.
            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, CreateTek(1))
            };

            Write(wfs);
        
            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(1, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Teks0_NothingToSeeHereMoveAlong()
        {
            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Empty(result.EksInfo);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_WorkflowFac.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Teks1_GetStuffed()
        {
            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Equal(1, result.EksInfo.Length);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Tek5_NotStuffed()
        {

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Equal(1, result.EksInfo.Length);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount); //TODO still should be 'published' by the snapshot
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount); 

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Tek10_NotStuffed()
        {
            var teks = Enumerable.Range(1, 10)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, teks)
            };

            Write(wfs);
            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(10, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(10, result.OutputCount);
            Assert.Equal(1, result.EksInfo.Length);
            Assert.Equal(10, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(WfToEks_EksBatchJobMk3Tests))]
        public void Tek11_NotStuffed_2Eks()
        {
            var teks = Enumerable.Range(1, 11)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_DateTimeProvider.Snapshot, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(11, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(11, result.OutputCount);
            Assert.Equal(2, result.EksInfo.Length);
            Assert.Equal(10, result.EksInfo[0].TekCount);
            Assert.Equal(1, result.EksInfo[1].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            //TODO Assert.Equal(_WorkflowFac.NewDbContextFunc().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);
            Assert.Equal(_DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }
    }
}