// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests
{
    public abstract class EksEngineTests : IDisposable
    {
        private readonly IWrappedEfExtensions _EfExtensions;
        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbProvider;
        private readonly IDbProvider<ContentDbContext> _ContentDbProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _EksPublishingJobDbProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;

        private readonly IUtcDateTimeProvider _Dtp = new StandardUtcDateTimeProvider();

        private readonly LoggerFactory _Lf;

        private readonly SnapshotWorkflowTeksToDksCommand _Snapshot;
        private readonly ExposureKeySetBatchJobMk3 _EksJob;
        private readonly ManifestUpdateCommand _ManifestJob;
        private readonly NlContentResignExistingV1ContentCommand _Resign;
        private StandardRandomNumberGenerator _Rng;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _CountriesOut;

        public EksEngineTests(IDbProvider<WorkflowDbContext> workflowDbProvider, IDbProvider<DkSourceDbContext> dkSourceDbProvider, IDbProvider<EksPublishingJobDbContext> eksPublishingJobDbProvider, IDbProvider<ContentDbContext> contentDbProvider, IWrappedEfExtensions efExtensions)
        {
            _Lf = new LoggerFactory();

            _EfExtensions = efExtensions;
            _WorkflowDbProvider = workflowDbProvider;
            _DkSourceDbProvider = dkSourceDbProvider;
            _EksPublishingJobDbProvider = eksPublishingJobDbProvider;
            _ContentDbProvider = contentDbProvider;

            ////Configuration
            var tekValidatorConfig = new Mock<ITekValidatorConfig>(MockBehavior.Strict);
            var eksHeaderConfig = new Mock<IEksHeaderInfoConfig>(MockBehavior.Strict);
            var eksConfig = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfig.Setup(x => x.TekCountMax).Returns(750000);
            eksConfig.Setup(x => x.LifetimeDays).Returns(14);

            var gaSigner = new Mock<IGaContentSigner>(MockBehavior.Strict);
            //gaSigner.Setup(x => x.SignatureOid).Returns("The OID");
            gaSigner.Setup(x => x.GetSignature(It.IsAny<byte[]>())).Returns(new byte[] { 1 });

            var nlSigner = new Mock<IContentSigner>(MockBehavior.Loose);
            nlSigner.Setup(x => x.GetSignature(new byte[0])).Returns(new byte[] { 2 });

            _Snapshot = new SnapshotWorkflowTeksToDksCommand(
                _Lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _Dtp,
                new TransmissionRiskLevelCalculationMk2(),
                _WorkflowDbProvider.CreateNew(),
                _WorkflowDbProvider.CreateNew,
                _DkSourceDbProvider.CreateNew,
                _EfExtensions,
                new IDiagnosticKeyProcessor[] {
                    
                });


            _CountriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            _CountriesOut.Setup(x => x.CountriesOfInterest).Returns(new[]{"ET"});
            _Rng = new StandardRandomNumberGenerator();
            _EksJob = new ExposureKeySetBatchJobMk3(
                eksConfig.Object,
                new EksBuilderV1(eksHeaderConfig.Object, gaSigner.Object, nlSigner.Object, _Dtp, new GeneratedProtobufEksContentFormatter(),
                    new EksBuilderV1LoggingExtensions(_Lf.CreateLogger<EksBuilderV1LoggingExtensions>())
                    ),
                _EksPublishingJobDbProvider.CreateNew,
                _Dtp,
                new EksEngineLoggingExtensions(_Lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _Rng, _Dtp, eksConfig.Object),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new TestLogger<SnapshotLoggingExtensions>()), _DkSourceDbProvider.CreateNew(), _EksPublishingJobDbProvider.CreateNew),
                new MarkDiagnosisKeysAsUsedLocally(_DkSourceDbProvider.CreateNew, eksConfig.Object, _EksPublishingJobDbProvider.CreateNew, _Lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_ContentDbProvider.CreateNew, _EksPublishingJobDbProvider.CreateNew, new Sha256HexPublishingIdService(), new EksJobContentWriterLoggingExtensions(_Lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_DkSourceDbProvider.CreateNew(), _EksPublishingJobDbProvider.CreateNew(),
                new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_CountriesOut.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()}
                ),
                _EfExtensions
            );

            var jsonSerializer = new StandardJsonSerializer();
            _ManifestJob = new ManifestUpdateCommand(
                new ManifestBuilder(_ContentDbProvider.CreateNew(), eksConfig.Object, _Dtp),
                new ManifestBuilderV3(_ContentDbProvider.CreateNew(), eksConfig.Object, _Dtp),
                new ManifestBuilderV4(_ContentDbProvider.CreateNew(), eksConfig.Object, _Dtp),
                _ContentDbProvider.CreateNew,
                new ManifestUpdateCommandLoggingExtensions(_Lf.CreateLogger<ManifestUpdateCommandLoggingExtensions>()),
                _Dtp,
                jsonSerializer,
                new StandardContentEntityFormatter(new ZippedSignedContentFormatter(nlSigner.Object), new Sha256HexPublishingIdService(), jsonSerializer),
                () => new StandardContentEntityFormatter(new ZippedSignedContentFormatter(nlSigner.Object), new Sha256HexPublishingIdService(), jsonSerializer)
            );

            var thumbmprintConfig = new Mock<IThumbprintConfig>(MockBehavior.Strict);
            _Resign = new NlContentResignExistingV1ContentCommand(
                new NlContentResignCommand(_ContentDbProvider.CreateNew, nlSigner.Object, new ResignerLoggingExtensions(_Lf.CreateLogger<ResignerLoggingExtensions>())));
        }

        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task EmptySystemNoTeks()
        {
            Assert.Equal(0, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            await _Snapshot.ExecuteAsync();
            await _EksJob.ExecuteAsync();
            await _ManifestJob.ExecuteAllAsync();
            await _Resign.ExecuteAsync();

            Assert.Equal(1, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(1, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.Manifest));

            Assert.Equal(0, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
        }

        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task EmptySystemSingleTek()
        {
            var workflowConfig = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfig.Setup(x => x.TimeToLiveMinutes).Returns(24*60*60); //Approx
            workflowConfig.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            Func<TekReleaseWorkflowStateCreate> createWf = () =>
                new TekReleaseWorkflowStateCreate(
                    _WorkflowDbProvider.CreateNewWithTx(),
                    _Dtp, 
                    _Rng,
                    new LabConfirmationIdService(_Rng),
                    new TekReleaseWorkflowTime(workflowConfig.Object),
                    workflowConfig.Object,
                    new RegisterSecretLoggingExtensions(_Lf.CreateLogger<RegisterSecretLoggingExtensions>())
                );

            await new GenerateTeksCommand(_Rng, _WorkflowDbProvider.CreateNewWithTx, createWf).ExecuteAsync(new GenerateTeksCommandArgs {TekCountPerWorkflow = 1, WorkflowCount = 1});

            Assert.Equal(1, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            await _Snapshot.ExecuteAsync(); //Too soon to publish TEKs
            await _EksJob.ExecuteAsync();
            await _ManifestJob.ExecuteAllAsync();
            await _Resign.ExecuteAsync();

            Assert.Equal(1, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(1, _ContentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.Manifest));
        }

        public void Dispose() 
        {
            _WorkflowDbProvider.Dispose();
            _ContentDbProvider.Dispose();
            _EksPublishingJobDbProvider.Dispose();
            _DkSourceDbProvider.Dispose();
        }
    }
}