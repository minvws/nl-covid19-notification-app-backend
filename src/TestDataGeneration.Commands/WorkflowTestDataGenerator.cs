// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Serilog.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands
{
    public class WorkflowTestDataGenerator
    {
        private readonly IDbProvider<WorkflowDbContext> _workflowDbContextProvider;
        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbContextProvider;
        private readonly ILoggerFactory _loggerFactory = new SerilogLoggerFactory();
        private readonly IUtcDateTimeProvider _utcDateTimeProvider = new StandardUtcDateTimeProvider();
        private readonly StandardRandomNumberGenerator _rng = new StandardRandomNumberGenerator();
        private readonly IWrappedEfExtensions _efExtensions;

        public WorkflowTestDataGenerator(IDbProvider<WorkflowDbContext> workflowDbContextProvider, IDbProvider<DkSourceDbContext> dkSourceDbContextProvider, IWrappedEfExtensions efExtensions)
        {
            _workflowDbContextProvider = workflowDbContextProvider ?? throw new ArgumentNullException(nameof(workflowDbContextProvider));
            _dkSourceDbContextProvider = dkSourceDbContextProvider ?? throw new ArgumentNullException(nameof(dkSourceDbContextProvider));
            _efExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
        }

        public async Task<int> GenerateAndAuthoriseWorkflowsAsync(int workflowCount = 25, int tekPerWOrkflowCount = 4)
        {
            await GenerateWorkflowsAsync(workflowCount, tekPerWOrkflowCount);
            await AuthoriseAllWorkflowsAsync();
            await SnapshotToDks();
            return _dkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count();
        }

        public async Task SnapshotToDks()
        {
            var countriesOutMock = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            countriesOutMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "DE" });

            await new SnapshotWorkflowTeksToDksCommand(_loggerFactory.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _utcDateTimeProvider,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowDbContextProvider.CreateNew(),
                _workflowDbContextProvider.CreateNew,
                _dkSourceDbContextProvider.CreateNew,
                _efExtensions,
                new IDiagnosticKeyProcessor[] {
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(countriesOutMock.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                }
            ).ExecuteAsync();
        }

        public async Task GenerateWorkflowsAsync(int workflowCount = 25, int tekPerWorkflowCount = 4)
        {
            var workflowConfigMock = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfigMock.Setup(x => x.TimeToLiveMinutes).Returns(10000);
            workflowConfigMock.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            var luhnModNGeneratorMock = new Mock<ILuhnModNGenerator>();
            var luhnModNConfig = new LuhnModNConfig();

            Func<TekReleaseWorkflowStateCreate> createWf = () =>
                new TekReleaseWorkflowStateCreate(
                    _workflowDbContextProvider.CreateNewWithTx(),
                    _utcDateTimeProvider,
                    _rng,
                    new LabConfirmationIdService(_rng),
                    new TekReleaseWorkflowTime(workflowConfigMock.Object),
                    new RegisterSecretLoggingExtensions(_loggerFactory.CreateLogger<RegisterSecretLoggingExtensions>())
                );

            var gen = new GenerateTeksCommand(_rng, _workflowDbContextProvider.CreateNewWithTx, createWf);
            await gen.ExecuteAsync(new GenerateTeksCommandArgs { WorkflowCount = workflowCount, TekCountPerWorkflow = tekPerWorkflowCount });

            if (workflowCount != _workflowDbContextProvider.CreateNew().KeyReleaseWorkflowStates.Count()) throw new InvalidOperationException();
            if (workflowCount * tekPerWorkflowCount != _workflowDbContextProvider.CreateNew().TemporaryExposureKeys.Count()) throw new InvalidOperationException();
        }

        public async Task AuthoriseAllWorkflowsAsync()
        {
            var wfdb = _workflowDbContextProvider.CreateNew();
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