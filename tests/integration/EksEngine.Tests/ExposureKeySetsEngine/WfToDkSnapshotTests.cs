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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    public abstract class WfToDkSnapshotTests
    {
        #region Implementation

        private readonly WorkflowDbContext _workflowContext;
        private readonly DkSourceDbContext _dkSourceContext;
        private readonly Mock<IUtcDateTimeProvider> _dateTimeProvider;

        protected WfToDkSnapshotTests(DbContextOptions<WorkflowDbContext> workflowContextOptions, DbContextOptions<DkSourceDbContext> dkSourceContextOptions)
        {
            _dateTimeProvider = new Mock<IUtcDateTimeProvider>();

            _workflowContext = new WorkflowDbContext(workflowContextOptions ?? throw new ArgumentNullException(nameof(workflowContextOptions)));
            _workflowContext.Database.EnsureCreated();
            _dkSourceContext = new DkSourceDbContext(dkSourceContextOptions ?? throw new ArgumentNullException(nameof(dkSourceContextOptions)));
            _dkSourceContext.Database.EnsureCreated();
        }

        private SnapshotWorkflowTeksToDksCommand Create()
        {
            return new SnapshotWorkflowTeksToDksCommand(new NullLogger<SnapshotWorkflowTeksToDksCommand>(),
                _dateTimeProvider.Object,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowContext,
                _dkSourceContext,
                Array.Empty<IDiagnosticKeyProcessor>()
            );
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            _workflowContext.KeyReleaseWorkflowStates.AddRange(workflows);
            _workflowContext.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            _workflowContext.SaveChanges();
            Assert.Equal(workflows.Length, _workflowContext.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count), _workflowContext.TemporaryExposureKeys.Count());
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

        #endregion

        [InlineData(0, 0, 120, 0)] //Null case
        [InlineData(1, 10, 119, 0)] //Just before
        [InlineData(1, 10, 120, 10)] //Exactly
        [InlineData(1, 10, 121, 10)] //After
        [Theory]
        public async Task PublishAfter(int wfCount, int tekPerWfCount, int addMins, int resultCount)
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();

            var t = new DateTime(2020, 11, 5, 12, 0, 0, DateTimeKind.Utc);
            _dateTimeProvider.Setup(x => x.Snapshot).Returns(t);
            var tekCount = wfCount * tekPerWfCount;
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            Assert.Equal(tekCount, _workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _dkSourceContext.DiagnosisKeys.Count());

            _dateTimeProvider.Setup(x => x.Snapshot).Returns(t.AddMinutes(addMins));
            var sut = Create();

            // Act
            var result = (SnapshotWorkflowTeksToDksResult)await sut.ExecuteAsync();

            // Assert
            Assert.Equal(resultCount, result.TekReadCount);
            Assert.Equal(tekCount - resultCount, _workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(resultCount, _dkSourceContext.DiagnosisKeys.Count());
        }

        [InlineData(0, 0, 0)] //Null case
        [InlineData(1, 1, 1)]
        [InlineData(1, 10, 10)]
        [Theory]
        public async Task SecondRunShouldChangeNothing(int wfCount, int tekPerWfCount, int resultCount)
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();

            _dateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 14, 00, 0, DateTimeKind.Utc));
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            //Two hours later
            _dateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 16, 00, 0, DateTimeKind.Utc));
            var tekCount = wfCount * tekPerWfCount;
            Assert.Equal(tekCount, _workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _dkSourceContext.DiagnosisKeys.Count());
            Assert.True(_dkSourceContext.DiagnosisKeys.All(x => x.DailyKey.RollingPeriod == UniversalConstants.RollingPeriodRange.Hi)); //Compatible with Apple API

            // Act
            var result = (SnapshotWorkflowTeksToDksResult)await Create().ExecuteAsync();

            // Assert
            Assert.Equal(resultCount, result.TekReadCount);
            Assert.Equal(resultCount, _workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(resultCount, _dkSourceContext.DiagnosisKeys.Count());

            //Second Act
            result = (SnapshotWorkflowTeksToDksResult)await Create().ExecuteAsync();

            //Assert No changes
            Assert.Equal(0, result.TekReadCount);
            Assert.Equal(resultCount, _workflowContext.TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(resultCount, _dkSourceContext.DiagnosisKeys.Count());
        }
    }
}
