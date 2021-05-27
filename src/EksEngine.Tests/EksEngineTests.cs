// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
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
        private readonly IWrappedEfExtensions _efExtensions;
        private readonly IDbProvider<WorkflowDbContext> _workflowDbProvider;
        private readonly IDbProvider<ContentDbContext> _contentDbProvider;
        private readonly IDbProvider<EksPublishingJobDbContext> _eksPublishingJobDbProvider;
        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbProvider;

        private readonly IUtcDateTimeProvider _dtp = new StandardUtcDateTimeProvider();

        private readonly LoggerFactory _lf;

        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;
        private readonly ExposureKeySetBatchJobMk3 _eksJob;
        private readonly ManifestUpdateCommand _manifestJob;
        private readonly NlContentResignExistingV1ContentCommand _resign;
        private readonly StandardRandomNumberGenerator _rng;

        public EksEngineTests(IDbProvider<WorkflowDbContext> workflowDbProvider, IDbProvider<DkSourceDbContext> dkSourceDbProvider, IDbProvider<EksPublishingJobDbContext> eksPublishingJobDbProvider, IDbProvider<ContentDbContext> contentDbProvider, IWrappedEfExtensions efExtensions)
        {
            _lf = new LoggerFactory();

            _efExtensions = efExtensions;
            _workflowDbProvider = workflowDbProvider;
            _dkSourceDbProvider = dkSourceDbProvider;
            _eksPublishingJobDbProvider = eksPublishingJobDbProvider;
            _contentDbProvider = contentDbProvider;

            // Configuration
            var eksHeaderConfig = new Mock<IEksHeaderInfoConfig>(MockBehavior.Strict);
            var eksConfig = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfig.Setup(x => x.TekCountMax).Returns(750000);
            eksConfig.Setup(x => x.LifetimeDays).Returns(14);

            var gaSigner = new Mock<IGaContentSigner>(MockBehavior.Strict);
            gaSigner.Setup(x => x.GetSignature(It.IsAny<byte[]>())).Returns(new byte[] { 1 });

            var nlSigner = new Mock<IContentSigner>(MockBehavior.Loose);
            nlSigner.Setup(x => x.GetSignature(new byte[0])).Returns(new byte[] { 2 });

            _snapshot = new SnapshotWorkflowTeksToDksCommand(
                _lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _dtp,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowDbProvider.CreateNew(),
                _workflowDbProvider.CreateNew,
                _dkSourceDbProvider.CreateNew,
                _efExtensions,
                new IDiagnosticKeyProcessor[] {}
            );

            var countriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            countriesOut.Setup(x => x.CountriesOfInterest).Returns(new[] { "ET" });
            _rng = new StandardRandomNumberGenerator();
            _eksJob = new ExposureKeySetBatchJobMk3(
                eksConfig.Object,
                new EksBuilderV1(eksHeaderConfig.Object, gaSigner.Object, nlSigner.Object, _dtp, new GeneratedProtobufEksContentFormatter(),
                    new EksBuilderV1LoggingExtensions(_lf.CreateLogger<EksBuilderV1LoggingExtensions>())
                    ),
                _eksPublishingJobDbProvider.CreateNew,
                _dtp,
                new EksEngineLoggingExtensions(_lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _rng, _dtp, eksConfig.Object),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new TestLogger<SnapshotLoggingExtensions>()), _dkSourceDbProvider.CreateNew(), _eksPublishingJobDbProvider.CreateNew,
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
                new MarkDiagnosisKeysAsUsedLocally(_dkSourceDbProvider.CreateNew, eksConfig.Object, _eksPublishingJobDbProvider.CreateNew, _lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentDbProvider.CreateNew, _eksPublishingJobDbProvider.CreateNew, new Sha256HexPublishingIdService(), new EksJobContentWriterLoggingExtensions(_lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_dkSourceDbProvider.CreateNew(), _eksPublishingJobDbProvider.CreateNew(),
                new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(countriesOut.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()}
                ),
                _efExtensions
                );

            var jsonSerializer = new StandardJsonSerializer();
            _manifestJob = new ManifestUpdateCommand(
                new ManifestV2Builder(_contentDbProvider.CreateNew(), eksConfig.Object, _dtp),
                new ManifestV3Builder(_contentDbProvider.CreateNew(), eksConfig.Object, _dtp),
                new ManifestV4Builder(_contentDbProvider.CreateNew(), eksConfig.Object, _dtp),
                _contentDbProvider.CreateNew,
                new ManifestUpdateCommandLoggingExtensions(_lf.CreateLogger<ManifestUpdateCommandLoggingExtensions>()),
                _dtp,
                jsonSerializer,
                () => new StandardContentEntityFormatter(new ZippedSignedContentFormatter(nlSigner.Object), new Sha256HexPublishingIdService(), jsonSerializer)
            );

            var thumbprintConfig = new Mock<IThumbprintConfig>(MockBehavior.Strict);
            _resign = new NlContentResignExistingV1ContentCommand(
                new NlContentResignCommand(_contentDbProvider.CreateNew, nlSigner.Object, new ResignerLoggingExtensions(_lf.CreateLogger<ResignerLoggingExtensions>())));
        }

        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task EmptySystemNoTeks()
        {
            Assert.Equal(0, _workflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            await _snapshot.ExecuteAsync();
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAllAsync();
            await _resign.ExecuteAsync();

            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.Manifest));

            Assert.Equal(0, _workflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
        }

        [Fact]
        [ExclusivelyUses(nameof(EksEngineTests))]
        public async Task EmptySystemSingleTek()
        {
            var workflowConfig = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfig.Setup(x => x.TimeToLiveMinutes).Returns(24 * 60 * 60); //Approx
            workflowConfig.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            var luhnModNConfig = new LuhnModNConfig();
            var luhnModNGeneratorMock = new Mock<ILuhnModNGenerator>();

            Func<TekReleaseWorkflowStateCreate> createWf = () =>
                new TekReleaseWorkflowStateCreate(
                    _workflowDbProvider.CreateNewWithTx(),
                    _dtp,
                    _rng,
                    new LabConfirmationIdService(_rng),
                    new TekReleaseWorkflowTime(workflowConfig.Object),
                    new RegisterSecretLoggingExtensions(_lf.CreateLogger<RegisterSecretLoggingExtensions>())
                );

            await new GenerateTeksCommand(_rng, _workflowDbProvider.CreateNewWithTx, createWf).ExecuteAsync(new GenerateTeksCommandArgs { TekCountPerWorkflow = 1, WorkflowCount = 1 });

            Assert.Equal(1, _workflowDbProvider.CreateNew().TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            await _snapshot.ExecuteAsync(); //Too soon to publish TEKs
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAllAsync();
            await _resign.ExecuteAsync();

            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(0, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.Manifest));
        }

        public void Dispose()
        {
            _workflowDbProvider.Dispose();
            _contentDbProvider.Dispose();
            _eksPublishingJobDbProvider.Dispose();
            _dkSourceDbProvider.Dispose();
        }
    }
}