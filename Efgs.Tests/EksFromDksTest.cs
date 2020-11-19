// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Serilog.Extensions.Logging;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Efgs.Tests
{
    /// <summary>
    /// Tests the command sequence for:
    /// Fake inbound IKS in DB
    /// Snapshot to DK Source
    /// Snapshot for EKS
    /// Build EKS
    /// </summary>
    public abstract class EksFromDksTest
    {
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider = new StandardUtcDateTimeProvider();
        private readonly StandardRandomNumberGenerator _Rng = new StandardRandomNumberGenerator();

        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbContextProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbContextProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _PublishingJobDbContextProvider;
        private readonly IDbProvider<ContentDbContext> _ContentDbContextProvider;

        private readonly ILoggerFactory _LoggerFactory = new SerilogLoggerFactory();
        private readonly IWrappedEfExtensions _EfExtensions;

        protected EksFromDksTest(IDbProvider<WorkflowDbContext> workflowDbContextProvider, IDbProvider<DkSourceDbContext> dkSourceDbContextProvider, IDbProvider<EksPublishingJobDbContext> publishingJobDbContextProvider, IDbProvider<ContentDbContext> contentDbContextProvider, IWrappedEfExtensions efExtensions)
        {
            _WorkflowDbContextProvider = workflowDbContextProvider ?? throw new ArgumentNullException(nameof(workflowDbContextProvider));
            _DkSourceDbContextProvider = dkSourceDbContextProvider ?? throw new ArgumentNullException(nameof(dkSourceDbContextProvider));
            _PublishingJobDbContextProvider = publishingJobDbContextProvider ?? throw new ArgumentNullException(nameof(publishingJobDbContextProvider));
            _ContentDbContextProvider = contentDbContextProvider ?? throw new ArgumentNullException(nameof(contentDbContextProvider));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
        }

        [Fact]
        public async Task Execute()
        {
            await GenerateWorkflows();

            var wfdb = _WorkflowDbContextProvider.CreateNew();
            var tekCount = wfdb.TemporaryExposureKeys.Count();
            Assert.True(25 <= tekCount && tekCount <= 100);
            Assert.Equal(25, wfdb.KeyReleaseWorkflowStates.Count());

            AuthoriseAllWorkflows();

            //Snapshot...
            var snapshot = new SnapshotWorkflowTeksToDksCommand(_LoggerFactory.CreateLogger<SnapshotWorkflowTeksToDksCommand>(), 
                _UtcDateTimeProvider, 
                new TransmissionRiskLevelCalculationMk2(),
                _WorkflowDbContextProvider.CreateNew(),
                _WorkflowDbContextProvider.CreateNew,
                _DkSourceDbContextProvider.CreateNew, 
                _EfExtensions,
                new IDiagnosticKeyProcessor[0]
                );

            Assert.Equal(0, _WorkflowDbContextProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published));

            await snapshot.ExecuteAsync();

            Assert.Equal(0, _WorkflowDbContextProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));

            Assert.True(_DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Where(x => x.PublishedLocally == false).Count() > 25);

            //This is all setting up the EKS Engine:

            //Mocks
            var eksConfigMock = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfigMock.Setup(x => x.LifetimeDays).Returns(14);
            eksConfigMock.Setup(x => x.TekCountMin).Returns(150);
            eksConfigMock.Setup(x => x.TekCountMax).Returns(750);
            eksConfigMock.Setup(x => x.PageSize).Returns(1000);

            var eksHeaderInfoMock = new Mock<IEksHeaderInfoConfig>(MockBehavior.Strict);
            eksHeaderInfoMock.Setup(x => x.AppBundleId).Returns("FakeAppBundleId");
            eksHeaderInfoMock.Setup(x => x.VerificationKeyId).Returns("FakeVerificationKeyId");
            eksHeaderInfoMock.Setup(x => x.VerificationKeyVersion).Returns("FakeVerificationKeyVersion");

            var tekValidatorConfigMock = new Mock<ITekValidatorConfig>(MockBehavior.Loose);
            tekValidatorConfigMock.Setup(x => x.RollingPeriodMax).Returns(UniversalConstants.RollingPeriodMax);
            tekValidatorConfigMock.Setup(x => x.MaxAgeDays).Returns(14);
            tekValidatorConfigMock.Setup(x => x.KeyDataLength).Returns(30);

            //Real objects:
            var lf = new LoggerFactory();
            var dtp = new StandardUtcDateTimeProvider();


            var le = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());

            var s1 = new EcdSaSigner(
                new EmbeddedResourceCertificateProvider(
                    new HardCodedCertificateLocationConfig("TestECDSA.p12", ""),
                    le));

            var s2 = new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(
                    new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"),
                    le),

                new EmbeddedResourcesCertificateChainProvider(new HardCodedCertificateLocationConfig("StaatDerNLChain-Expires2020-08-28.p7b", "")),
                
                dtp);

            var sut = new EksBuilderV1(
                eksHeaderInfoMock.Object, 
                s1, 
                s2,
                dtp,
                new GeneratedProtobufEksContentFormatter(),
                new EksBuilderV1LoggingExtensions(lf.CreateLogger<EksBuilderV1LoggingExtensions>())
                );

            var eksJob = new ExposureKeySetBatchJobMk3(
                eksConfigMock.Object,
                sut,
                _PublishingJobDbContextProvider.CreateNew,
                _UtcDateTimeProvider,
                new EksEngineLoggingExtensions(_LoggerFactory.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _Rng, _UtcDateTimeProvider, eksConfigMock.Object),
                new SnapshotDiagnosisKeys(_LoggerFactory.CreateLogger<SnapshotDiagnosisKeys>(), _DkSourceDbContextProvider.CreateNew(), _PublishingJobDbContextProvider.CreateNew),
                new MarkDiagnosisKeysAsUsed(_DkSourceDbContextProvider.CreateNew, eksConfigMock.Object, _PublishingJobDbContextProvider.CreateNew, _LoggerFactory.CreateLogger<MarkDiagnosisKeysAsUsed>()),
                new EksJobContentWriter(_ContentDbContextProvider.CreateNew, _PublishingJobDbContextProvider.CreateNew, new Sha256HexPublishingIdService(), new EksJobContentWriterLoggingExtensions(_LoggerFactory.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_DkSourceDbContextProvider.CreateNew(), _PublishingJobDbContextProvider.CreateNew()),
                _EfExtensions
                );

            var result = await eksJob.ExecuteAsync();

            Assert.Equal(tekCount, result.InputCount);
            Assert.True(result.StuffingCount > 0);
            Assert.Equal(150, result.OutputCount);

            Assert.Equal(1, result.EksInfo.Length);

            Assert.Equal(0, result.ReconcileEksSumCount);
            Assert.Equal(0, result.ReconcileOutputCount);

            var eksResult = result.EksInfo[0];
            Assert.Equal(150, eksResult.TekCount);

            Assert.Equal(150, _DkSourceDbContextProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedLocally));
        }

        private async Task GenerateWorkflows()
        {
            var m1 = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            m1.Setup(x => x.TimeToLiveMinutes).Returns(10000);
            m1.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);
            m1.Setup(x => x.BucketIdLength).Returns(30);
            m1.Setup(x => x.ConfirmationKeyLength).Returns(30);

            Func<TekReleaseWorkflowStateCreate> offs = () =>
                new TekReleaseWorkflowStateCreate(_WorkflowDbContextProvider.CreateNewWithTx(),
                    _UtcDateTimeProvider, _Rng,
                    new LabConfirmationIdService(_Rng),
                    new TekReleaseWorkflowTime(m1.Object),
                    m1.Object,
                    new RegisterSecretLoggingExtensions(_LoggerFactory.CreateLogger<RegisterSecretLoggingExtensions>())
                    );


            var gen = new GenerateTeksCommand(_Rng, _WorkflowDbContextProvider.CreateNew, offs);
            await gen.ExecuteAsync(new GenerateTeksCommandArgs {TekCountPerWorkflow = 4, WorkflowCount = 25});
        }

        private void AuthoriseAllWorkflows()
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

            wfdb.SaveChanges();
        }
    }
}
