// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Serilog.Extensions.Logging;
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
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbContextProvider;
        private readonly IDbProvider<IksInDbContext> _IksInDbContextProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbContextProvider;
        private readonly IDbProvider<IksPublishingJobDbContext> _IksPublishingJobDbContextProvider;
        private readonly IDbProvider<IksOutDbContext> _IksOutDbContextProvider;
        
        private readonly IWrappedEfExtensions _EfExtensions;

        private readonly ILoggerFactory _LoggerFactory = new SerilogLoggerFactory();

        private readonly Mock<IIksConfig> _IksConfigMock = new Mock<IIksConfig>(MockBehavior.Strict);
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _CountriesConfigMock = new Mock<IOutboundFixedCountriesOfInterestSetting>(MockBehavior.Strict);
        private readonly Mock<IUtcDateTimeProvider> _UtcDateTimeProviderMock = new Mock<IUtcDateTimeProvider>(MockBehavior.Strict);

        protected IksEngineTest(IDbProvider<WorkflowDbContext> workflowDbContextProvider, IDbProvider<IksInDbContext> iksInDbContextProvider, IDbProvider<DkSourceDbContext> dkSourceDbContextProvider, IDbProvider<IksPublishingJobDbContext> iksPublishingJobDbContextProvider, IDbProvider<IksOutDbContext> iksOutDbContextProvider, IWrappedEfExtensions efExtensions)
        {
            _IksInDbContextProvider = iksInDbContextProvider ?? throw new ArgumentNullException(nameof(iksInDbContextProvider));
            _DkSourceDbContextProvider = dkSourceDbContextProvider ?? throw new ArgumentNullException(nameof(dkSourceDbContextProvider));
            _IksPublishingJobDbContextProvider = iksPublishingJobDbContextProvider ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContextProvider));
            _IksOutDbContextProvider = iksOutDbContextProvider ?? throw new ArgumentNullException(nameof(iksOutDbContextProvider));
            _WorkflowDbContextProvider = workflowDbContextProvider ?? throw new ArgumentNullException(nameof(workflowDbContextProvider));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
        }

        private IksEngine Create()
        {
            _IksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _IksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _CountriesConfigMock.Setup(x => x.CountriesOfInterest).Returns(new []{"GB", "AU"});
            return new IksEngine(
                _LoggerFactory.CreateLogger<IksEngine>(),
                new IksInputSnapshotCommand(_LoggerFactory.CreateLogger<IksInputSnapshotCommand>(), _DkSourceDbContextProvider.CreateNew(), _IksPublishingJobDbContextProvider.CreateNew, _CountriesConfigMock.Object),
                new IksFormatter(),
                _IksConfigMock.Object,
                _UtcDateTimeProviderMock.Object,
                new MarkDiagnosisKeysAsUsedByIks(_DkSourceDbContextProvider.CreateNew, _IksConfigMock.Object, _IksPublishingJobDbContextProvider.CreateNew, _LoggerFactory.CreateLogger<MarkDiagnosisKeysAsUsedByIks>()),
                new IksJobContentWriter(_IksOutDbContextProvider.CreateNew, _IksPublishingJobDbContextProvider.CreateNew, _LoggerFactory.CreateLogger<IksJobContentWriter>()),
                _IksPublishingJobDbContextProvider.CreateNew,
                _EfExtensions
            );
        }


        [InlineData(2)]
        [Theory]
        [ExclusivelyUses(nameof(IksEngineTest))]
        public async Task Execute(int iksCount)
        {
            //Mocks
            _IksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _IksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            GenerateIks(iksCount);

            Assert.Equal(iksCount, _IksInDbContextProvider.CreateNew().Received.Count(x => x.Accepted == null));
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally == false));
            Assert.Equal(0, _IksOutDbContextProvider.CreateNew().Iks.Count());

            //Act
            var result = await Create().ExecuteAsync();

            //TODO Assert.Equal(tekCount, result.InputCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.Items.Length);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            //Don't publish DKs from EFGS
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
        }

        private void GenerateIks(int iksCount)
        {
            //Add an IKS or 2.
            var idk = new InteropKeyFormatterArgs
            {
                TransmissionRiskLevel = 1,
                CountriesOfInterest = new[] {"DE"},
                ReportType = EfgsReportType.ConfirmedTest,
                Origin = "DE",
                DaysSinceSymtpomsOnset = 0,
                Value = new DailyKey
                {
                    RollingStartNumber = _UtcDateTimeProviderMock.Object.Snapshot.Date.ToRollingStartNumber(),
                    RollingPeriod = UniversalConstants.RollingPeriodRange.Hi,
                    KeyData = new byte[UniversalConstants.DailyKeyDataByteCount]
                }
            };

            var input = Enumerable.Range(0, iksCount).Select(_ =>
                new IksInEntity
                {
                    Created = _UtcDateTimeProviderMock.Object.Snapshot,
                    BatchTag = "argle",
                    Content = new IksFormatter().Format(new[] {idk}),
                    //Accepted = 
                }).ToArray();

            var iksInDb = _IksInDbContextProvider.CreateNew();
            iksInDb.Received.AddRange(input);
            iksInDb.SaveChanges();
        }

        [Fact]
        [ExclusivelyUses(nameof(IksEngineTest))]
        public async Task Empty()
        {
            //Mocks
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            Assert.Equal(0, _IksInDbContextProvider.CreateNew().Received.Count());
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _IksOutDbContextProvider.CreateNew().Iks.Count());

            //Act
            var result = await Create().ExecuteAsync();

            Assert.Equal(0, result.InputCount);
            Assert.Equal(0, result.OutputCount);
            Assert.Equal(0, result.Items.Length);
            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);
            Assert.Equal(0, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _IksOutDbContextProvider.CreateNew().Iks.Count());
        }

        [Fact]
        [ExclusivelyUses(nameof(IksEngineTest))]
        public async Task ExecuteFromWorkflows()
        {
            //Mocks
            _IksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _IksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            var usableDkCount = await new WorkflowTestDataGenerator(
                _WorkflowDbContextProvider,
                _DkSourceDbContextProvider,
                _EfExtensions
            ).GenerateAndAuthoriseWorkflowsAsync();

            //Act
            var result = await Create().ExecuteAsync();

            Assert.Equal(usableDkCount, result.InputCount);
            Assert.Equal(usableDkCount, result.OutputCount); //No filters...
            Assert.Equal(1, result.Items.Length);

            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            var itemResult = result.Items[0];
            Assert.Equal(usableDkCount, itemResult.ItemCount);

            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _IksOutDbContextProvider.CreateNew().Iks.Count());
        }

        [Fact]
        [ExclusivelyUses(nameof(IksEngineTest))]
        public async Task ExecuteFromWorkflowsTwice()
        {
            //Mocks
            _IksConfigMock.Setup(x => x.ItemCountMax).Returns(750);
            _IksConfigMock.Setup(x => x.PageSize).Returns(1000);
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 16, 15, 14, 13, DateTimeKind.Utc));

            var usableDkCount = await new WorkflowTestDataGenerator(
                _WorkflowDbContextProvider,
                _DkSourceDbContextProvider,
                _EfExtensions
            ).GenerateAndAuthoriseWorkflowsAsync();

            //Act
            var result = await Create().ExecuteAsync();

            Assert.Equal(usableDkCount, result.InputCount);
            Assert.Equal(usableDkCount, result.OutputCount); //No filters...
            Assert.Equal(1, result.Items.Length);

            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            var itemResult = result.Items[0];
            Assert.Equal(usableDkCount, itemResult.ItemCount);

            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _IksOutDbContextProvider.CreateNew().Iks.Count());

            //Act
            var result2 = await Create().ExecuteAsync();
            Assert.Equal(0, result2.InputCount);
            Assert.Equal(0, result2.OutputCount); //No filters...
            Assert.Equal(0, result2.Items.Length);
            Assert.Equal(0, result2.ReconcileEksSumCount);
            Assert.Equal(0, result2.ReconcileOutputCount);
            //Unchanged
            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(usableDkCount, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(1, _IksOutDbContextProvider.CreateNew().Iks.Count());
        }
    }
}
