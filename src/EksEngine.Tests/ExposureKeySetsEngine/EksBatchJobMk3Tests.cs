// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
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
    public abstract class EksBatchJobMk3Tests
    {
        private readonly FakeEksConfig _fakeEksConfig;
        private readonly WorkflowDbContext _workflowContext;
        private readonly DkSourceDbContext _dkSourceContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _outboundCountriesMock;
        private readonly LoggerFactory _lf;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;
        private ExposureKeySetBatchJobMk3 _engine;

        protected EksBatchJobMk3Tests(DbContextOptions<WorkflowDbContext> workflowContextOptions, DbContextOptions<DkSourceDbContext> dkSourceDbContextOptions, DbContextOptions<EksPublishingJobDbContext> eksPublishingJobDbContextOptions, DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _workflowContext = new WorkflowDbContext(workflowContextOptions);
            _workflowContext.Database.EnsureCreated();
            _dkSourceContext = new DkSourceDbContext(dkSourceDbContextOptions);
            _dkSourceContext.Database.EnsureCreated();
            _eksPublishingJobDbContext = new EksPublishingJobDbContext(eksPublishingJobDbContextOptions);
            _eksPublishingJobDbContext.Database.EnsureCreated();
            _contentDbContext = new ContentDbContext(contentDbContextOptions);
            _contentDbContext.Database.EnsureCreated();

            _dateTimeProvider = new StandardUtcDateTimeProvider();
            _fakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };
            _lf = new LoggerFactory();

            _snapshot = new SnapshotWorkflowTeksToDksCommand(_lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                new StandardUtcDateTimeProvider(),
                new TransmissionRiskLevelCalculationMk2(),
                _workflowContext,
                _dkSourceContext,
                new IDiagnosticKeyProcessor[] { }
            );

            _outboundCountriesMock = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            _outboundCountriesMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "CY", "BG" });
        }

        private async Task Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            await _workflowContext.KeyReleaseWorkflowStates.AddRangeAsync(workflows);
            await _workflowContext.TemporaryExposureKeys.AddRangeAsync(workflows.SelectMany(x => x.Teks));
            await _workflowContext.SaveChangesAsync();

            Assert.Equal(workflows.Length, _workflowContext.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count), _workflowContext.TemporaryExposureKeys.Count());

            await _snapshot.ExecuteAsync();
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
        private async Task<EksEngineResult> RunEngine()
        {
            _engine = new ExposureKeySetBatchJobMk3(
                _fakeEksConfig,
                new FakeEksBuilder(),
                _eksPublishingJobDbContext,
                new StandardUtcDateTimeProvider(),
                new EksEngineLoggingExtensions(_lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), new StandardRandomNumberGenerator(), _dateTimeProvider, _fakeEksConfig),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new TestLogger<SnapshotLoggingExtensions>()), _dkSourceContext, _eksPublishingJobDbContext,
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
                new MarkDiagnosisKeysAsUsedLocally(_dkSourceContext, _fakeEksConfig, _eksPublishingJobDbContext, _lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentDbContext, _eksPublishingJobDbContext, new Sha256HexPublishingIdService(),
                    new EksJobContentWriterLoggingExtensions(_lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_dkSourceContext, _eksPublishingJobDbContext,
                    new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_outboundCountriesMock.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                    }
                    )
                );

            return await _engine.ExecuteAsync();
        }

        private class FakeEksConfig : IEksConfig
        {
            public int LifetimeDays { get; set; } = 14;
            public int TekCountMax { get; set; } = 20;
            public int TekCountMin { get; set; } = 10;
            public int PageSize { get; set; } = 100;
            public bool CleanupDeletesData => throw new NotImplementedException();
        }

        private class FakeEksBuilder : IEksBuilder
        {
            public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys) => new byte[] { 1 };
        }

        [Fact]
        public async Task FireSameEngineTwice()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            await RunEngine();
            Assert.Throws<InvalidOperationException>(() => _engine.ExecuteAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public async Task FireTwice()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(1, result.FilteredInputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.NotEmpty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);

            result = await RunEngine();

            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(5, _dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally));

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Teks0_NothingToSeeHereMoveAlong()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            // Act
            var result = await RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.FilteredInputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Empty(result.EksInfo);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Teks1_GetStuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek5_NotStuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek5_Stuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks)
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek5_AsymptomaticNotStuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(0).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks)
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek5_Asymptomatic2Stuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = new List<TekEntity>();
            teks.AddRange(Enumerable.Range(1, 3).Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-1).ToRollingStartNumber()))); // dsos = 0
            teks.AddRange(Enumerable.Range(1, 2).Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))); // dsos = -1

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Asymptomatic, teks.ToArray())
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek10_NotStuffed()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = Enumerable.Range(1, 10)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public async Task Tek11_NotStuffed_2Eks()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            var teks = Enumerable.Range(1, 11)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, InfectiousPeriodType.Symptomatic, teks)
            };

            await Write(wfs);

            // Act
            var result = await RunEngine();

            // Assert
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

            Assert.Equal(_contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        private async Task BulkDeleteAllDataInTest()
        {
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _workflowContext.BulkDeleteAsync(_workflowContext.TemporaryExposureKeys.ToList());
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());
            await _dkSourceContext.BulkDeleteAsync(_dkSourceContext.DiagnosisKeys.ToList());
            await _dkSourceContext.BulkDeleteAsync(_dkSourceContext.DiagnosisKeysInput.ToList());
            await _eksPublishingJobDbContext.BulkDeleteAsync(_eksPublishingJobDbContext.EksInput.ToList());
            await _eksPublishingJobDbContext.BulkDeleteAsync(_eksPublishingJobDbContext.EksOutput.ToList());
        }
    }
}
