// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands;
using Xunit;
using EfgsReportType = Iks.Protobuf.EfgsReportType;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    /// <summary>
    /// Tests the command sequence for:
    /// Fake inbound IKS in DB
    /// Snapshot to DK Source
    /// Snapshot for EKS
    /// Build EKS
    /// </summary>
    public abstract class IksEngineTest
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly IksInDbContext _iksInDbContext;
        private readonly DiagnosisKeysDbContext _diagnosisKeysDbContext;
        private readonly IksPublishingJobDbContext _iksPublishingJobDbContext;
        private readonly IksOutDbContext _iksOutDbContext;

        private readonly Mock<IIksConfig> _iksConfigMock = new Mock<IIksConfig>(MockBehavior.Strict);
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _countriesConfigMock = new Mock<IOutboundFixedCountriesOfInterestSetting>(MockBehavior.Strict);
        private readonly Mock<IUtcDateTimeProvider> _utcDateTimeProviderMock = new Mock<IUtcDateTimeProvider>(MockBehavior.Strict);

        protected IksEngineTest(DbContextOptions<WorkflowDbContext> workflowDbContextOptions, DbContextOptions<IksInDbContext> iksInDbContextOptions, DbContextOptions<DiagnosisKeysDbContext> diagnosisKeysDbContextOptions, DbContextOptions<IksPublishingJobDbContext> iksPublishingJobDbContextOptions, DbContextOptions<IksOutDbContext> iksOutDbContextOptions)
        {
            _iksInDbContext = new IksInDbContext(iksInDbContextOptions ?? throw new ArgumentNullException(nameof(iksInDbContextOptions)));
            _iksInDbContext.Database.EnsureCreated();
            _diagnosisKeysDbContext = new DiagnosisKeysDbContext(diagnosisKeysDbContextOptions ?? throw new ArgumentNullException(nameof(diagnosisKeysDbContextOptions)));
            _diagnosisKeysDbContext.Database.EnsureCreated();
            _iksPublishingJobDbContext = new IksPublishingJobDbContext(iksPublishingJobDbContextOptions ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContextOptions)));
            _iksPublishingJobDbContext.Database.EnsureCreated();
            _iksOutDbContext = new IksOutDbContext(iksOutDbContextOptions ?? throw new ArgumentNullException(nameof(iksOutDbContextOptions)));
            _iksOutDbContext.Database.EnsureCreated();
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions ?? throw new ArgumentNullException(nameof(workflowDbContextOptions)));
            _workflowDbContext.Database.EnsureCreated();
        }

        private IksEngine Create()
        {
            _iksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _iksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _countriesConfigMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "GB", "AU" });
            return new IksEngine(
                new NullLogger<IksEngine>(),
                new IksInputSnapshotCommand(new NullLogger<IksInputSnapshotCommand>(), _diagnosisKeysDbContext, _iksPublishingJobDbContext, _countriesConfigMock.Object),
                new IksFormatter(),
                _iksConfigMock.Object,
                _utcDateTimeProviderMock.Object,
                new MarkDiagnosisKeysAsUsedByIks(_diagnosisKeysDbContext, _iksConfigMock.Object, _iksPublishingJobDbContext, new NullLogger<MarkDiagnosisKeysAsUsedByIks>()),
                new IksJobContentWriter(_iksOutDbContext, _iksPublishingJobDbContext, new NullLogger<IksJobContentWriter>()),
                _iksPublishingJobDbContext
            );
        }

        [InlineData(2)]
        [Theory]
        public async Task Execute(int iksCount)
        {
            // Arrange
            await _diagnosisKeysDbContext.BulkDeleteAsync(_diagnosisKeysDbContext.DiagnosisKeys.ToList());
            await _iksInDbContext.BulkDeleteAsync(_iksInDbContext.InJob.ToList());
            await _iksOutDbContext.BulkDeleteAsync(_iksOutDbContext.Iks.ToList());

            //Mocks
            _iksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _iksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            GenerateIks(iksCount);

            Assert.Equal(iksCount, _iksInDbContext.Received.Count(x => x.Accepted == null));
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedLocally == false));
            Assert.Equal(0, _iksOutDbContext.Iks.Count());

            //Act
            var result = (IksEngineResult)await Create().ExecuteAsync();

            Assert.Equal(0, result.OutputCount);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            //Don't publish DKs from EFGS
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));
        }

        private void GenerateIks(int iksCount)
        {
            //Add an IKS or 2.
            var idk = new InteropKeyFormatterArgs
            {
                TransmissionRiskLevel = 1,
                CountriesOfInterest = new[] { "DE" },
                ReportType = EfgsReportType.ConfirmedTest,
                Origin = "DE",
                DaysSinceSymtpomsOnset = 0,
                Value = new DailyKey
                {
                    RollingStartNumber = _utcDateTimeProviderMock.Object.Snapshot.Date.ToRollingStartNumber(),
                    RollingPeriod = UniversalConstants.RollingPeriodRange.Hi,
                    KeyData = new byte[UniversalConstants.DailyKeyDataByteCount]
                }
            };

            var input = Enumerable.Range(0, iksCount).Select(_ =>
                new IksInEntity
                {
                    Created = _utcDateTimeProviderMock.Object.Snapshot,
                    BatchTag = "argle",
                    Content = new IksFormatter().Format(new[] { idk }),
                    //Accepted = 
                }).ToArray();

            _iksInDbContext.Received.AddRange(input);
            _iksInDbContext.SaveChanges();
        }

        [Fact]
        public async Task Empty()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            //Mocks
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            Assert.Equal(0, _iksInDbContext.Received.Count());
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(0, _iksOutDbContext.Iks.Count());

            //Act
            var result = (IksEngineResult)await Create().ExecuteAsync();

            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(0, _iksOutDbContext.Iks.Count());
        }

        [Fact]
        public async Task ExecuteFromWorkflows()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            //Mocks
            _iksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _iksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            var usableDkCount = await new WorkflowTestDataGenerator(
                _workflowDbContext,
                _diagnosisKeysDbContext
            ).GenerateAndAuthoriseWorkflowsAsync();

            //Act
            var result = (IksEngineResult)await Create().ExecuteAsync();

            Assert.Equal(usableDkCount, result.InputCount);
            Assert.Equal(usableDkCount, result.OutputCount); //No filters...
            Assert.Single(result.Items);

            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            var itemResult = result.Items[0];
            Assert.Equal(usableDkCount, itemResult.ItemCount);

            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _iksOutDbContext.Iks.Count());
        }

        [Fact]
        public async Task ExecuteFromWorkflowsTwice()
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            //Mocks
            _iksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _iksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            var usableDkCount = await new WorkflowTestDataGenerator(
                _workflowDbContext,
                _diagnosisKeysDbContext
            ).GenerateAndAuthoriseWorkflowsAsync();

            //Act
            var result = (IksEngineResult)await Create().ExecuteAsync();

            Assert.Equal(usableDkCount, result.InputCount);
            Assert.Equal(usableDkCount, result.OutputCount); //No filters...
            Assert.Single(result.Items);

            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            var itemResult = result.Items[0];
            Assert.Equal(usableDkCount, itemResult.ItemCount);

            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _iksOutDbContext.Iks.Count());

            //Act
            var result2 = (IksEngineResult)await Create().ExecuteAsync();
            Assert.Equal(0, result2.InputCount);
            Assert.Equal(0, result2.OutputCount); //No filters...
            Assert.Empty(result2.Items);
            Assert.Equal(0, result2.ReconcileEksSumCount);
            Assert.Equal(0, result2.ReconcileOutputCount);
            //Unchanged
            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _iksOutDbContext.Iks.Count());
        }

        private async Task BulkDeleteAllDataInTest()
        {
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            await _diagnosisKeysDbContext.BulkDeleteAsync(_diagnosisKeysDbContext.DiagnosisKeys.ToList());
            await _iksInDbContext.BulkDeleteAsync(_iksInDbContext.InJob.ToList());
            await _iksInDbContext.BulkDeleteAsync(_iksInDbContext.Received.ToList());
            await _iksOutDbContext.BulkDeleteAsync(_iksOutDbContext.Iks.ToList());
        }
    }
}
