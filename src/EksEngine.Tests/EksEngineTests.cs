// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
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
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests
{
    public abstract class EksEngineTests
    {
        private readonly WorkflowDbContext _workflowContext;
        private readonly DkSourceDbContext _dkSourceContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobContext;
        private readonly ContentDbContext _contentDbContext;

        private readonly IUtcDateTimeProvider _dtp = new StandardUtcDateTimeProvider();

        private readonly LoggerFactory _lf;

        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;
        private readonly ExposureKeySetBatchJobMk3 _eksJob;
        private readonly ManifestUpdateCommand _manifestJob;
        private readonly NlContentResignExistingV1ContentCommand _resign;
        private readonly StandardRandomNumberGenerator _rng;

        protected EksEngineTests(DbContextOptions<WorkflowDbContext> workflowContextOptions, DbContextOptions<DkSourceDbContext> dkSourceContextOptions, DbContextOptions<EksPublishingJobDbContext> eksPublishingJobDbContextOptions, DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _lf = new LoggerFactory();

            _workflowContext = new WorkflowDbContext(workflowContextOptions);
            _workflowContext.Database.EnsureCreated();
            _dkSourceContext = new DkSourceDbContext(dkSourceContextOptions);
            _dkSourceContext.Database.EnsureCreated();
            _eksPublishingJobContext = new EksPublishingJobDbContext(eksPublishingJobDbContextOptions);
            _eksPublishingJobContext.Database.EnsureCreated();
            _contentDbContext = new ContentDbContext(contentDbContextOptions);
            _contentDbContext.Database.EnsureCreated();

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
                _workflowContext,
                _dkSourceContext,
                new IDiagnosticKeyProcessor[] { }
            );

            var countriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            countriesOut.Setup(x => x.CountriesOfInterest).Returns(new[] { "ET" });
            _rng = new StandardRandomNumberGenerator();
            _eksJob = new ExposureKeySetBatchJobMk3(
                eksConfig.Object,
                new EksBuilderV1(eksHeaderConfig.Object, gaSigner.Object, nlSigner.Object, _dtp, new GeneratedProtobufEksContentFormatter(),
                    new EksBuilderV1LoggingExtensions(_lf.CreateLogger<EksBuilderV1LoggingExtensions>())
                    ),
                _eksPublishingJobContext,
                _dtp,
                new EksEngineLoggingExtensions(_lf.CreateLogger<EksEngineLoggingExtensions>()),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _rng, _dtp, eksConfig.Object),
                new SnapshotDiagnosisKeys(new SnapshotLoggingExtensions(new NullLogger<SnapshotLoggingExtensions>()), _dkSourceContext, _eksPublishingJobContext, new Infectiousness(new Dictionary<InfectiousPeriodType, HashSet<int>>{
                        {
                            InfectiousPeriodType.Symptomatic,
                            new HashSet<int>() { -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        },
                        {
                            InfectiousPeriodType.Asymptomatic,
                            new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        }
                    })),
                new MarkDiagnosisKeysAsUsedLocally(_dkSourceContext, eksConfig.Object, _eksPublishingJobContext, _lf.CreateLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentDbContext, _eksPublishingJobContext, new Sha256HexPublishingIdService(), new EksJobContentWriterLoggingExtensions(_lf.CreateLogger<EksJobContentWriterLoggingExtensions>())),
                new WriteStuffingToDiagnosisKeys(_dkSourceContext, _eksPublishingJobContext,
                new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(countriesOut.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()}
                ));

            var jsonSerializer = new StandardJsonSerializer();
            _manifestJob = new ManifestUpdateCommand(
                new ManifestV2Builder(_contentDbContext, eksConfig.Object, _dtp),
                new ManifestV3Builder(_contentDbContext, eksConfig.Object, _dtp),
                new ManifestV4Builder(_contentDbContext, eksConfig.Object, _dtp),
                _contentDbContext,
                new ManifestUpdateCommandLoggingExtensions(_lf.CreateLogger<ManifestUpdateCommandLoggingExtensions>()),
                _dtp,
                jsonSerializer,
                () => new StandardContentEntityFormatter(new ZippedSignedContentFormatter(nlSigner.Object), new Sha256HexPublishingIdService(), jsonSerializer)
            );

            var thumbprintConfig = new Mock<IThumbprintConfig>(MockBehavior.Strict);
            _resign = new NlContentResignExistingV1ContentCommand(
                new NlContentResignCommand(_contentDbContext, nlSigner.Object, new ResignerLoggingExtensions(_lf.CreateLogger<ResignerLoggingExtensions>())));
        }

        [Fact]
        public async Task EmptySystemNoTeks()
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();

            Assert.Equal(0, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceContext.DiagnosisKeys.Count());

            // Act
            await _snapshot.ExecuteAsync();
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAllAsync();
            await _resign.ExecuteAsync();

            // Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.Manifest));

            Assert.Equal(0, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceContext.DiagnosisKeys.Count());
        }

        [Fact]
        public async Task EmptySystemSingleTek()
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();
            await _contentDbContext.TruncateAsync<ContentEntity>();

            var workflowConfig = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfig.Setup(x => x.TimeToLiveMinutes).Returns(24 * 60 * 60); //Approx
            workflowConfig.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            var luhnModNConfig = new LuhnModNConfig();
            var luhnModNGenerator = new LuhnModNGenerator(luhnModNConfig);

            await new GenerateTeksCommand(_workflowContext, _rng, _dtp, new TekReleaseWorkflowTime(workflowConfig.Object), luhnModNConfig, luhnModNGenerator, _lf.CreateLogger<GenerateTeksCommand>()).ExecuteAsync(new GenerateTeksCommandArgs { TekCountPerWorkflow = 1, WorkflowCount = 1 });

            Assert.Equal(1, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _dkSourceContext.DiagnosisKeys.Count());

            // Act
            await _snapshot.ExecuteAsync(); //Too soon to publish TEKs
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAllAsync();
            await _resign.ExecuteAsync();

            // Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2));
            //Obsolete - replace with raw content
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.Manifest));
        }
    }
}
