// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GenerateTeks.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands
{
    public class WorkflowTestDataGenerator
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly DkSourceDbContext _dkSourceDbContext;
        private readonly IUtcDateTimeProvider _utcDateTimeProvider = new StandardUtcDateTimeProvider();
        private readonly StandardRandomNumberGenerator _rng = new StandardRandomNumberGenerator();

        public WorkflowTestDataGenerator(WorkflowDbContext workflowDbContext, DkSourceDbContext dkSourceDbContext)
        {
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
        }

        public async Task<int> GenerateAndAuthoriseWorkflowsAsync(int workflowCount = 25, int tekPerWOrkflowCount = 4)
        {
            await GenerateWorkflowsAsync(workflowCount, tekPerWOrkflowCount);
            await AuthoriseAllWorkflowsAsync();
            await SnapshotToDks();
            return _dkSourceDbContext.DiagnosisKeys.Count();
        }

        private async Task SnapshotToDks()
        {
            var countriesOutMock = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            countriesOutMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "DE" });

            await new SnapshotWorkflowTeksToDksCommand(new NullLogger<SnapshotWorkflowTeksToDksCommand>(),
                _utcDateTimeProvider,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowDbContext,
                _dkSourceDbContext,
                new IDiagnosticKeyProcessor[] {
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(countriesOutMock.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                }
            ).ExecuteAsync();
        }

        private async Task GenerateWorkflowsAsync(int workflowCount = 25, int tekPerWorkflowCount = 4)
        {
            var workflowConfigMock = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfigMock.Setup(x => x.TimeToLiveMinutes).Returns(10000);
            workflowConfigMock.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            var luhnModNConfig = new LuhnModNConfig();
            var luhnModNGenerator = new LuhnModNGenerator(luhnModNConfig);

            var gen = new GenerateTeksCommand(_workflowDbContext, _rng, _utcDateTimeProvider, new TekReleaseWorkflowTime(workflowConfigMock.Object), luhnModNConfig, luhnModNGenerator, new NullLogger<GenerateTeksCommand>());
            await gen.ExecuteAsync(new GenerateTeksCommandArgs { WorkflowCount = workflowCount, TekCountPerWorkflow = tekPerWorkflowCount });

            if (workflowCount != _workflowDbContext.KeyReleaseWorkflowStates.Count())
            {
                throw new InvalidOperationException();
            }

            if (workflowCount * tekPerWorkflowCount != _workflowDbContext.TemporaryExposureKeys.Count())
            {
                throw new InvalidOperationException();
            }
        }

        private async Task AuthoriseAllWorkflowsAsync()
        {
            var wfdb = _workflowDbContext;
            foreach (var i in wfdb.KeyReleaseWorkflowStates)
            {
                i.AuthorisedByCaregiver = _utcDateTimeProvider.Snapshot;
                i.StartDateOfTekInclusion = _utcDateTimeProvider.Snapshot.Date.AddDays(-2);
            }

            foreach (var i in wfdb.TemporaryExposureKeys)
            {
                i.PublishAfter = _utcDateTimeProvider.Snapshot;
            }

            await wfdb.SaveChangesAsync();
        }
    }
}
