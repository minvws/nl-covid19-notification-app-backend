// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    public abstract class EksBatchJobMk3Tests : IDisposable
    {
        #region Implementation

        private readonly FakeEksConfig _fakeEksConfig;
        private readonly IDbProvider<WorkflowDbContext> _workflowFac;
        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _eksPublishingJobDbProvider;
        private readonly IDbProvider<ContentDbContext> _contentDbProvider;
        private readonly IWrappedEfExtensions _efExtensions;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _outboundCountriesMock;

        private readonly LoggerFactory _lf;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        private ExposureKeySetBatchJobMk3 _engine;
        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;

        protected EksBatchJobMk3Tests(IDbProvider<WorkflowDbContext> workflowFac, IDbProvider<DkSourceDbContext> dkSourceFac, IDbProvider<EksPublishingJobDbContext> publishingFac, IDbProvider<ContentDbContext> contentFac, IWrappedEfExtensions efExtensions)
        {
            _workflowFac = workflowFac ?? throw new ArgumentNullException(nameof(workflowFac));
            _dkSourceDbProvider = dkSourceFac ?? throw new ArgumentNullException(nameof(dkSourceFac));
            _eksPublishingJobDbProvider = publishingFac ?? throw new ArgumentNullException(nameof(publishingFac));
            _contentDbProvider = contentFac ?? throw new ArgumentNullException(nameof(contentFac));
            _efExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));

            _dateTimeProvider = new StandardUtcDateTimeProvider();
            _fakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };
            _lf = new LoggerFactory();

            _snapshot = new SnapshotWorkflowTeksToDksCommand(_lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                new StandardUtcDateTimeProvider(),
                new TransmissionRiskLevelCalculationMk2(),
                _workflowFac.CreateNew(),
                _workflowFac.CreateNew(),
                _dkSourceDbProvider.CreateNew,
                _efExtensions,
                new IDiagnosticKeyProcessor[] { }
            );

            _outboundCountriesMock = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            _outboundCountriesMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "CY", "BG" });
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            var db = _workflowFac.CreateNew();
            db.KeyReleaseWorkflowStates.AddRange(workflows);
            db.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            db.SaveChanges();

            Assert.Equal(workflows.Length, db.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count), db.TemporaryExposureKeys.Count());

            _snapshot.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static TekEntity CreateTek(int rsn)
        {
            return new TekEntity { RollingStartNumber = rsn, RollingPeriod = 2, KeyData = new byte[16], PublishAfter = DateTime.UtcNow.AddHours(-1) };
        }

        private static TekReleaseWorkflowStateEntity Create(DateTime now, InfectiousPeriodType symptomatic, params TekEntity[] items)
        {
            return new TekReleaseWorkflowStateEntity
            {
                BucketId = new byte[0],
                ConfirmationKey = new byte[0],
                AuthorisedByCaregiver = now.AddHours(1),
                Created = now,
                ValidUntil = now.AddDays(1),
                StartDateOfTekInclusion = now.AddDays(-1).Date,
                IsSymptomatic = symptomatic,
                Teks = items
            };
        }

        private EksEngineResult RunEngine()
        {
            _engine = new ExposureKeySetBatchJobMk3(
                _fakeEksConfig,
                new FakeEksBuilder(),
                _eksPublishingJobDbProvider.CreateNew,
                new StandardUtcDateTimeProvider(),
                new EksEngineLoggingExtensions(_lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), new StandardRandomNumberGenerator(), _dateTimeProvider, _fakeEksConfig),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new TestLogger<SnapshotLoggingExtensions>()), _dkSourceDbProvider.CreateNew(), _eksPublishingJobDbProvider.CreateNew,
                    new Infectiousness(new Dictionary<InfectiousPeriodType, HashSet<int>>{
                        {
                            InfectiousPeriodType.Symptomatic,
                            new HashSet<int>() { -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        },
                        {
                            InfectiousPeriodType.Asymptomatic,
                            new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        }
                    })),
                new MarkDiagnosisKeysAsUsedLocally(_dkSourceDbProvider.CreateNew, _fakeEksConfig, _eksPublishingJobDbProvider.CreateNew, _lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentDbProvider.CreateNew, _eksPublishingJobDbProvider.CreateNew, new Sha256HexPublishingIdService(),
                    new EksJobContentWriterLoggingExtensions(_lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_dkSourceDbProvider.CreateNew(), _eksPublishingJobDbProvider.CreateNew(),
                    new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_outboundCountriesMock.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                    }
                    ),
                _efExtensions);

            return _engine.ExecuteAsync().GetAwaiter().GetResult();
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
            _workflowFac.Dispose();
            _dkSourceDbProvider.Dispose();
            _eksPublishingJobDbProvider.Dispose();
            _contentDbProvider.Dispose();
            _lf.Dispose();
        }

        #endregion

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void FireSameEngineTwice()
        {
            RunEngine();
            Assert.Throws<InvalidOperationException>(() => _engine.ExecuteAsync().GetAwaiter().GetResult());
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void FireTwice()
        {
            //One TEK from the dawn of time.
            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            var result = RunEngine();
            var firstInputCount = result.InputCount;
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(1, result.FilteredInputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.NotEmpty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);

            result = RunEngine();

            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(5, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Teks0_NothingToSeeHereMoveAlong()
        {
            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Empty(result.EksInfo);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_workflowFac.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Teks1_GetStuffed()
        {
            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(1, result.FilteredInputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek5_NotStuffed()
        {

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(5, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek5_Stuffed()
        {

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(0, result.FilteredInputCount);
            Assert.Equal(5, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(5, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek5_AsymptomaticNotStuffed()
        {
            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(0).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(5, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount); //InputCount + StuffingCount - TransmissionRiskNoneCount - OutputCount;
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek5_Asymptomatic2Stuffed()
        {
            var teks = new List<TekEntity>();
            teks.AddRange(Enumerable.Range(1, 3).Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-1).ToRollingStartNumber()))); // dsos = 0
            teks.AddRange(Enumerable.Range(1, 2).Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))); // dsos = -1

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks.ToArray())
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(3, result.FilteredInputCount);
            Assert.Equal(2, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(2, result.ReconcileOutputCount); //InputCount + StuffingCount - TransmissionRiskNoneCount - OutputCount;
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek10_NotStuffed()
        {
            var teks = Enumerable.Range(1, 10)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            Write(wfs);
            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(10, result.InputCount);
            Assert.Equal(10, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(10, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(10, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        [ExclusivelyUses(nameof(EksBatchJobMk3Tests))]
        public void Tek11_NotStuffed_2Eks()
        {
            var teks = Enumerable.Range(1, 11)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(11, result.InputCount);
            Assert.Equal(11, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(11, result.OutputCount);
            Assert.Equal(2, result.EksInfo.Length);
            Assert.Equal(10, result.EksInfo[0].TekCount);
            Assert.Equal(1, result.EksInfo[1].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }
    }
}
