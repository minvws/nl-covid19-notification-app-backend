// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    public abstract class TekToDkSnapshotTests
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly LoggerFactory _lf;
        private readonly Mock<IUtcDateTimeProvider> _dateTimeProvider;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _outboundCountries;

        protected TekToDkSnapshotTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions, DbContextOptions<DkSourceDbContext> dkSourceDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions ?? throw new ArgumentNullException(nameof(workflowDbContextOptions)));
            _workflowDbContext.Database.EnsureCreated();
            _dkSourceDbContext = new DkSourceDbContext(dkSourceDbContextOptions ?? throw new ArgumentNullException(nameof(dkSourceDbContextOptions)));
            _dkSourceDbContext.Database.EnsureCreated();

            _dateTimeProvider = new Mock<IUtcDateTimeProvider>();
            _outboundCountries = new Mock<IOutboundFixedCountriesOfInterestSetting>(MockBehavior.Strict);
            _lf = new LoggerFactory();
        }

        private SnapshotWorkflowTeksToDksCommand Create()
        {
            _outboundCountries.Setup(x => x.CountriesOfInterest).Returns(new[] { "GB" });

            return new SnapshotWorkflowTeksToDksCommand(_lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _dateTimeProvider.Object,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowDbContext,
                _dkSourceDbContext,
                new IDiagnosticKeyProcessor[] {
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_outboundCountries.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                },
                new DiagnosiskeyInputEntityDeduplicator(
                    _dkSourceDbContext,
                    _lf.CreateLogger<DiagnosiskeyInputEntityDeduplicator>()));
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            _workflowDbContext.KeyReleaseWorkflowStates.AddRange(workflows);
            _workflowDbContext.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            _workflowDbContext.SaveChanges();
            Assert.Equal(workflows.Length, _workflowDbContext.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count), _workflowDbContext.TemporaryExposureKeys.Count());
        }

        private void GenerateWorkflowTeks(int wfCount, int tekPerWfCount)
        {
            Write(Enumerable.Range(0, wfCount).Select(x => GenWorkflow(x, GenTeks(tekPerWfCount))).ToArray());
        }

        private TekEntity[] GenTeks(int tekPerWfCount)
        {
            var t = _dateTimeProvider.Object.Snapshot;
            return Enumerable.Range(0, tekPerWfCount).Select(x =>
                new TekEntity
                {
                    RollingStartNumber = t.AddDays(-x).ToUniversalTime().Date.ToRollingStartNumber(),
                    RollingPeriod = 2, //Corrected by a processor.
                    KeyData = new byte[UniversalConstants.DailyKeyDataByteCount],
                    PublishAfter = t.AddHours(2)
                }
            ).ToArray();
        }

        private TekReleaseWorkflowStateEntity GenWorkflow(int key, params TekEntity[] items)
        {
            var now = _dateTimeProvider.Object.Snapshot;

            var b = BitConverter.GetBytes(key);

            return new TekReleaseWorkflowStateEntity
            {

                BucketId = b,
                ConfirmationKey = b,
                AuthorisedByCaregiver = now,
                Created = now,
                ValidUntil = now.AddDays(1),
                StartDateOfTekInclusion = now.AddDays(-1).Date, //Yesterday
                IsSymptomatic = InfectiousPeriodType.Symptomatic,
                Teks = items
            };
        }

        [InlineData(0, 0, 120, 0)] //Null case
        [InlineData(1, 10, 119, 0)] //Just before
        [InlineData(1, 10, 120, 10)] //Exactly
        [InlineData(1, 10, 121, 10)] //After
        [Theory]
        public async Task PublishAfter(int wfCount, int tekPerWfCount, int addMins, int resultCount)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceDbContext.BulkDeleteAsync(_dkSourceDbContext.DiagnosisKeys.ToList());

            var t = new DateTime(2020, 11, 5, 12, 0, 0, DateTimeKind.Utc);
            _dateTimeProvider.Setup(x => x.Snapshot).Returns(t);
            var tekCount = wfCount * tekPerWfCount;
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            Assert.Equal(tekCount, _workflowDbContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _dkSourceDbContext.DiagnosisKeys.Count());

            _dateTimeProvider.Setup(x => x.Snapshot).Returns(t.AddMinutes(addMins));
            var c = Create();

            // Act
            var result = (SnapshotWorkflowTeksToDksResult)await c.ExecuteAsync();

            // Assert
            Assert.Equal(resultCount, result.TekReadCount);
            Assert.Equal(tekCount - resultCount, _workflowDbContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(result.DkCount, _dkSourceDbContext.DiagnosisKeys.Count(x => x.Local.TransmissionRiskLevel != TransmissionRiskLevel.None));
        }

        [InlineData(0, 0)] //Null case
        [InlineData(1, 1)]
        [InlineData(1, 4)] //was 10 - 6 are now filtered out as they have TRL None
        [Theory]
        public async Task SecondRunShouldChangeNothing(int wfCount, int tekPerWfCount)
        {
            // Arrange
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceDbContext.BulkDeleteAsync(_dkSourceDbContext.DiagnosisKeys.ToList());

            _dateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 14, 00, 0, DateTimeKind.Utc));
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            //Two hours later
            _dateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 16, 00, 0, DateTimeKind.Utc));
            var tekCount = wfCount * tekPerWfCount;
            Assert.Equal(tekCount, _workflowDbContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _dkSourceDbContext.DiagnosisKeys.Count());
            Assert.True(_dkSourceDbContext.DiagnosisKeys.All(x => x.DailyKey.RollingPeriod == UniversalConstants.RollingPeriodRange.Hi)); //Compatible with Apple API

            // Act
            var result = (SnapshotWorkflowTeksToDksResult)await Create().ExecuteAsync();

            // Assert
            Assert.Equal(tekCount, result.TekReadCount);
            Assert.Equal(tekCount, _workflowDbContext.TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(tekCount, _dkSourceDbContext.DiagnosisKeys.Count());

            //Second Act
            result = (SnapshotWorkflowTeksToDksResult)await Create().ExecuteAsync();

            //  Assert No changes
            Assert.Equal(0, result.TekReadCount);
            Assert.Equal(tekCount, _workflowDbContext.TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(tekCount, _dkSourceDbContext.DiagnosisKeys.Count());
        }
    }
}
