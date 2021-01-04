// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Serilog.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands
{
    public class WorkflowTestDataGenerator
    {
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbContextProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbContextProvider;
        private readonly ILoggerFactory _LoggerFactory = new SerilogLoggerFactory();
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
        private readonly StandardRandomNumberGenerator _Rng = new StandardRandomNumberGenerator();
        private readonly IWrappedEfExtensions _EfExtensions;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _CountriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();

        public WorkflowTestDataGenerator(IDbProvider<WorkflowDbContext> workflowDbContextProvider, IDbProvider<DkSourceDbContext> dkSourceDbContextProvider, IWrappedEfExtensions efExtensions)
        {
            _WorkflowDbContextProvider = workflowDbContextProvider ?? throw new ArgumentNullException(nameof(workflowDbContextProvider));
            _DkSourceDbContextProvider = dkSourceDbContextProvider ?? throw new ArgumentNullException(nameof(dkSourceDbContextProvider));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
        }

        public async Task<int> GenerateAndAuthoriseWorkflowsAsync(int workflowCount = 25, int tekPerWOrkflowCount = 4)
        {
            await GenerateWorkflowsAsync(workflowCount, tekPerWOrkflowCount);
            await AuthoriseAllWorkflowsAsync();
            await SnapshotToDks();
            return _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count();
        }

        public async Task SnapshotToDks()
        {
            _CountriesOut.Setup(x => x.CountriesOfInterest).Returns(new[] { "DE" });
            await new SnapshotWorkflowTeksToDksCommand(_LoggerFactory.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _UtcDateTimeProvider,
                new TransmissionRiskLevelCalculationMk2(),
                _WorkflowDbContextProvider.CreateNew(),
                _WorkflowDbContextProvider.CreateNew,
                _DkSourceDbContextProvider.CreateNew,
                _EfExtensions,
                new IDiagnosticKeyProcessor[] {
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_CountriesOut.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                }
            ).ExecuteAsync();
        }

        public async Task GenerateWorkflowsAsync(int workflowCount = 25, int tekPerWOrkflowCount = 4)
        {
            var m1 = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            m1.Setup(x => x.TimeToLiveMinutes).Returns(10000);
            m1.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);
            m1.Setup(x => x.BucketIdLength).Returns(30);
            m1.Setup(x => x.ConfirmationKeyLength).Returns(30);

            Func<TekReleaseWorkflowStateCreate> createWf = () =>
                new TekReleaseWorkflowStateCreate(_WorkflowDbContextProvider.CreateNewWithTx(),
                    _UtcDateTimeProvider, _Rng,
                    new LabConfirmationIdService(_Rng),
                    new TekReleaseWorkflowTime(m1.Object),
                    m1.Object,
                    new RegisterSecretLoggingExtensions(_LoggerFactory.CreateLogger<RegisterSecretLoggingExtensions>())
                );

            var gen = new GenerateTeksCommand(_Rng, _WorkflowDbContextProvider.CreateNew, createWf);
            await gen.ExecuteAsync(new GenerateTeksCommandArgs { WorkflowCount = workflowCount , TekCountPerWorkflow = tekPerWOrkflowCount });

            if (workflowCount != _WorkflowDbContextProvider.CreateNew().KeyReleaseWorkflowStates.Count()) throw new InvalidOperationException();
            if (workflowCount * tekPerWOrkflowCount != _WorkflowDbContextProvider.CreateNew().TemporaryExposureKeys.Count()) throw new InvalidOperationException();
        }

        public async Task AuthoriseAllWorkflowsAsync()
        {
            var wfdb = _WorkflowDbContextProvider.CreateNew();
            foreach (var i in wfdb.KeyReleaseWorkflowStates)
            {
                i.AuthorisedByCaregiver = _UtcDateTimeProvider.Snapshot;
                i.DateOfSymptomsOnset = _UtcDateTimeProvider.Snapshot.Date.AddDays(-2);
            }

            foreach (var i in wfdb.TemporaryExposureKeys)
            {
                i.PublishAfter = _UtcDateTimeProvider.Snapshot;
            }

            await wfdb.SaveChangesAsync();
        }
    }
}