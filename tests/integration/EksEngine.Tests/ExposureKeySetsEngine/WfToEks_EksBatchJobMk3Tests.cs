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
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
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
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    /// <summary>
    /// NB Change to use a DK source rather than WFs
    /// </summary>
    public abstract class WfToEksEksBatchJobMk3Tests
    {
        #region Implementation

        private readonly FakeEksConfig _fakeEksConfig;
        private readonly WorkflowDbContext _workflowContext;
        private readonly DkSourceDbContext _dkSourceContext;
        private readonly DbContextOptions<DkSourceDbContext> _dkSourceContextOptions;
        private readonly DbContextOptions<EksPublishingJobDbContext> _publishingContextOptions;
        private readonly ContentDbContext _contentContext;

        private readonly LoggerFactory _lf;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        private ExposureKeySetBatchJobMk3 _engine;
        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;
        private Mock<IOutboundFixedCountriesOfInterestSetting> _countriesOut;

        protected WfToEksEksBatchJobMk3Tests(DbContextOptions<WorkflowDbContext> workflowContextOptions, DbContextOptions<DkSourceDbContext> dkSourceContextOptions, DbContextOptions<EksPublishingJobDbContext> publishingContextOptions, DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _dkSourceContextOptions = dkSourceContextOptions ?? throw new ArgumentNullException(nameof(dkSourceContextOptions));
            _publishingContextOptions = publishingContextOptions ?? throw new ArgumentNullException(nameof(publishingContextOptions));

            _workflowContext = new WorkflowDbContext(workflowContextOptions ?? throw new ArgumentNullException(nameof(workflowContextOptions)));
            _workflowContext.Database.EnsureCreated();
            _dkSourceContext = new DkSourceDbContext(_dkSourceContextOptions);
            _dkSourceContext.Database.EnsureCreated();
            _contentContext = new ContentDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _contentContext.Database.EnsureCreated();

            _dateTimeProvider = new StandardUtcDateTimeProvider();
            _fakeEksConfig = new FakeEksConfig { LifetimeDays = 14, PageSize = 1000, TekCountMax = 10, TekCountMin = 5 };
            _lf = new LoggerFactory();

            _snapshot = new SnapshotWorkflowTeksToDksCommand(_lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                new StandardUtcDateTimeProvider(),
                new TransmissionRiskLevelCalculationMk2(),
                _workflowContext,
                _dkSourceContext,
                new IDiagnosticKeyProcessor[0]
            );
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            _workflowContext.KeyReleaseWorkflowStates.AddRange(workflows);
            _workflowContext.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            _workflowContext.SaveChanges();

            Assert.Equal(workflows.Length, _workflowContext.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count), _workflowContext.TemporaryExposureKeys.Count());

            _snapshot.ExecuteAsync().GetAwaiter().GetResult();
        }

        private static TekEntity CreateTek(int rsn)
        {
            return new TekEntity { RollingStartNumber = rsn, RollingPeriod = 2, KeyData = new byte[16], PublishAfter = DateTime.UtcNow.AddHours(-1) };
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
                StartDateOfTekInclusion = now.AddDays(-1).Date,
                IsSymptomatic = InfectiousPeriodType.Symptomatic,
                Teks = items
            };
        }

        private EksEngineResult RunEngine()
        {
            var dkSourceContext = new DkSourceDbContext(_dkSourceContextOptions);
            dkSourceContext.Database.EnsureCreated();

            var eksPublishingJobContext = new EksPublishingJobDbContext(_publishingContextOptions);
            eksPublishingJobContext.Database.EnsureCreated();

            _countriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            _countriesOut.Setup(x => x.CountriesOfInterest).Returns(new[] { "ET" });
            _engine = new ExposureKeySetBatchJobMk3(
                _fakeEksConfig,
                new FakeEksBuilder(),
                eksPublishingJobContext,
                new StandardUtcDateTimeProvider(),
                new EksEngineLoggingExtensions(_lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(),
                    new StandardRandomNumberGenerator(), _dateTimeProvider, _fakeEksConfig),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new NullLogger<SnapshotLoggingExtensions>()),
                   _dkSourceContext, eksPublishingJobContext,
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
                new MarkDiagnosisKeysAsUsedLocally(_dkSourceContext, _fakeEksConfig,
                    eksPublishingJobContext, _lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentContext, eksPublishingJobContext,
                    new Sha256HexPublishingIdService(),
                    new EksJobContentWriterLoggingExtensions(_lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_dkSourceContext,
                    eksPublishingJobContext,
                    new IDiagnosticKeyProcessor[]
                    {
                        new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_countriesOut.Object),
                        new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                    }
                )
                );

            return (EksEngineResult) _engine.ExecuteAsync().GetAwaiter().GetResult();
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

        #endregion

        [Fact]
        public void FireSameEngineTwice()
        {
            RunEngine();
            Assert.Throws<InvalidOperationException>(() => _engine.ExecuteAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public void FireTwice()
        {
            // Arrange
            _contentContext.Truncate<ContentEntity>();
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            //One TEK from the dawn of time.
            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            // Act
            var result = RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.NotEmpty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);

            result = RunEngine();

            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);
            Assert.Empty(result.EksInfo);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(1, _contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet)); // 2nd has only stuffing
            Assert.Equal(5, _dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally)); // 2nd run adds 0 stuffing records

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public void Teks0_NothingToSeeHereMoveAlong()
        {
            // Arrange
            _contentContext.Truncate<ContentEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            // Act
            var result = RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public void Teks1_GetStuffed()
        {
            // Arrange
            _contentContext.Truncate<ContentEntity>();
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
            };

            Write(wfs);

            // Act
            var result = RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(1, result.InputCount);
            Assert.Equal(4, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount + result.StuffingCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public void Tek5_NotStuffed()
        {
            _contentContext.Truncate<ContentEntity>();
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            var teks = Enumerable.Range(1, 5)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, teks)
            };

            Write(wfs);

            var result = RunEngine();
            Assert.True(result.Started > new DateTime(2020, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(5, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(5, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(5, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public void Tek10_NotStuffed()
        {
            // Arrange
            _contentContext.Truncate<ContentEntity>();
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            var teks = Enumerable.Range(1, 10)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, teks)
            };

            Write(wfs);

            // Act
            var result = RunEngine();

            // Assert
            Assert.True(result.Started > new DateTime(2020, 8, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.Equal(10, result.InputCount);
            Assert.Equal(0, result.StuffingCount);
            Assert.Equal(10, result.OutputCount);
            Assert.Single(result.EksInfo);
            Assert.Equal(10, result.EksInfo[0].TekCount);
            Assert.Equal(0, result.TransmissionRiskNoneCount);

            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, result.ReconcileEksSumCount);

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }

        [Fact]
        public void Tek11_NotStuffed_2Eks()
        {
            // Arrange
            _contentContext.Truncate<ContentEntity>();
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();
            _workflowContext.BulkDelete(_workflowContext.KeyReleaseWorkflowStates.ToList());

            var teks = Enumerable.Range(1, 11)
                .Select(x => CreateTek(DateTime.UtcNow.Date.AddDays(-2).ToRollingStartNumber()))
                .ToArray();

            var wfs = new[]
            {
                Create(_dateTimeProvider.Snapshot, teks)
            };

            Write(wfs);

            // Act
            var result = RunEngine();

            // Assert
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

            Assert.Equal(_contentContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet), result.EksInfo.Length);
            Assert.Equal(_dkSourceContext.DiagnosisKeys.Count(x => x.PublishedLocally), result.InputCount);

            Assert.True(result.TotalSeconds > 0);
        }
    }
}
