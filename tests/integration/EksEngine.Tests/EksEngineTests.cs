// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
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
using NL.Rijksoverheid.ExposureNotification.BackEnd.GenerateTeks.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests
{
    public abstract class EksEngineTests
    {
        private readonly WorkflowDbContext _workflowContext;
        private readonly DiagnosisKeysDbContext _diagnosisKeysContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobContext;
        private readonly ContentDbContext _contentDbContext;

        private readonly IUtcDateTimeProvider _dtp = new StandardUtcDateTimeProvider();

        private readonly SnapshotWorkflowTeksToDksCommand _snapshot;
        private readonly ExposureKeySetBatchJobMk3 _eksJob;
        private readonly ManifestUpdateCommand _manifestJob;
        private readonly StandardRandomNumberGenerator _rng;

        protected EksEngineTests(DbContextOptions<WorkflowDbContext> workflowContextOptions, DbContextOptions<DiagnosisKeysDbContext> diagnosisKeysContextOptions, DbContextOptions<EksPublishingJobDbContext> eksPublishingJobDbContextOptions, DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _workflowContext = new WorkflowDbContext(workflowContextOptions ?? throw new ArgumentNullException(nameof(workflowContextOptions)));
            _workflowContext.Database.EnsureCreated();
            _diagnosisKeysContext = new DiagnosisKeysDbContext(diagnosisKeysContextOptions ?? throw new ArgumentNullException(nameof(diagnosisKeysContextOptions)));
            _diagnosisKeysContext.Database.EnsureCreated();
            _eksPublishingJobContext = new EksPublishingJobDbContext(eksPublishingJobDbContextOptions ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContextOptions)));
            _eksPublishingJobContext.Database.EnsureCreated();
            _contentDbContext = new ContentDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _contentDbContext.Database.EnsureCreated();

            // Configuration
            var eksHeaderConfig = new Mock<IEksHeaderInfoConfig>(MockBehavior.Strict);
            eksHeaderConfig.Setup(x => x.VerificationKeyId).Returns("unittest");
            eksHeaderConfig.Setup(x => x.VerificationKeyVersion).Returns("unittest");
            eksHeaderConfig.Setup(x => x.AppBundleId).Returns("unittest");

            var eksConfig = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfig.Setup(x => x.TekCountMax).Returns(750000);
            eksConfig.Setup(x => x.TekCountMin).Returns(150);
            eksConfig.Setup(x => x.PageSize).Returns(10000);
            eksConfig.Setup(x => x.LifetimeDays).Returns(14);

            var gaSigner = new Mock<IGaContentSigner>(MockBehavior.Strict);
            gaSigner.Setup(x => x.SignatureOid).Returns("unittest");
            gaSigner.Setup(x => x.GetSignature(It.IsAny<byte[]>())).Returns(new byte[] { 1 });

            var gaV15Signer = new Mock<IGaContentSigner>(MockBehavior.Strict);
            gaV15Signer.Setup(x => x.SignatureOid).Returns("unittestV15");
            gaV15Signer.Setup(x => x.GetSignature(It.IsAny<byte[]>())).Returns(new byte[] { 1 });

            var nlSigner = new Mock<IContentSigner>(MockBehavior.Loose);
            nlSigner.Setup(x => x.GetSignature(new byte[0])).Returns(new byte[] { 2 });

            var hsmSignerService = new Mock<IHsmSignerService>();
            hsmSignerService.Setup(x => x.GetCmsSignatureAsync(new byte[0])).ReturnsAsync(new byte[] { 2 });
            hsmSignerService.Setup(x => x.GetGaenSignatureAsync(It.IsAny<byte[]>())).ReturnsAsync(new byte[] { 1 });

            _snapshot = new SnapshotWorkflowTeksToDksCommand(
                new NullLogger<SnapshotWorkflowTeksToDksCommand>(),
                _dtp,
                new TransmissionRiskLevelCalculationMk2(),
                _workflowContext,
                _diagnosisKeysContext,
                Array.Empty<IDiagnosticKeyProcessor>()
            );

            var countriesOut = new Mock<IOutboundFixedCountriesOfInterestSetting>();
            countriesOut.Setup(x => x.CountriesOfInterest).Returns(new[] { "ET" });
            _rng = new StandardRandomNumberGenerator();
            _eksJob = new ExposureKeySetBatchJobMk3(
                eksConfig.Object,
                new EksBuilderV1(
                    eksHeaderConfig.Object,
                    gaSigner.Object,
                    gaV15Signer.Object,
                    nlSigner.Object,
                    _dtp,
                    new GeneratedProtobufEksContentFormatter(),
                    hsmSignerService.Object,
                    new NullLogger<EksBuilderV1>()
                    ),
                _eksPublishingJobContext,
                _dtp,
                new NullLogger<ExposureKeySetBatchJobMk3>(),
                new EksStuffingGeneratorMk2(new TransmissionRiskLevelCalculationMk2(), _rng, _dtp, eksConfig.Object),
                new SnapshotDiagnosisKeys(new NullLogger<SnapshotDiagnosisKeys>(), _diagnosisKeysContext, _eksPublishingJobContext, new Infectiousness(new Dictionary<InfectiousPeriodType, HashSet<int>>{
                        {
                            InfectiousPeriodType.Symptomatic,
                            new HashSet<int>() { -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        },
                        {
                            InfectiousPeriodType.Asymptomatic,
                            new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
                        }
                    })),
                new MarkDiagnosisKeysAsUsedLocally(_diagnosisKeysContext, eksConfig.Object, _eksPublishingJobContext, new NullLogger<MarkDiagnosisKeysAsUsedLocally>()),
                new EksJobContentWriter(_contentDbContext, _eksPublishingJobContext, new NullLogger<EksJobContentWriter>()),
                new WriteStuffingToDiagnosisKeys(_diagnosisKeysContext, _eksPublishingJobContext,
                new IDiagnosticKeyProcessor[] {
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(countriesOut.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()}
                ));

            var jsonSerializer = new StandardJsonSerializer();
            _manifestJob = new ManifestUpdateCommand(
                new ManifestV4Builder(_contentDbContext, eksConfig.Object, _dtp),
                new ManifestV5Builder(_contentDbContext, eksConfig.Object, _dtp),
                _contentDbContext,
                new NullLogger<ManifestUpdateCommand>(),
                _dtp,
                jsonSerializer,
                new StandardContentEntityFormatter(
                    new ZippedSignedContentFormatter(
                        hsmSignerService.Object),
                    jsonSerializer)
            );
        }

        [Fact]
        public async Task EmptySystemNoTeks()
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _diagnosisKeysContext.TruncateAsync<DiagnosisKeyEntity>();
            await _contentDbContext.TruncateAsync<ContentEntity>();

            Assert.Equal(0, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _diagnosisKeysContext.DiagnosisKeys.Count());

            // Act
            await _snapshot.ExecuteAsync();
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAsync();

            // Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ManifestV4));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2)); // Stuffing not added

            Assert.Equal(0, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _diagnosisKeysContext.DiagnosisKeys.Count());
        }

        [Fact]
        public async Task EmptySystemSingleTek()
        {
            // Arrange
            await _workflowContext.BulkDeleteAsync(_workflowContext.KeyReleaseWorkflowStates.ToList());
            await _diagnosisKeysContext.TruncateAsync<DiagnosisKeyEntity>();
            await _contentDbContext.TruncateAsync<ContentEntity>();

            var workflowConfig = new Mock<IWorkflowConfig>(MockBehavior.Strict);
            workflowConfig.Setup(x => x.TimeToLiveMinutes).Returns(24 * 60 * 60); //Approx
            workflowConfig.Setup(x => x.PermittedMobileDeviceClockErrorMinutes).Returns(30);

            var luhnModNConfig = new LuhnModNConfig();
            var luhnModNGenerator = new LuhnModNGenerator(luhnModNConfig);

            await new GenerateTeksCommand(_workflowContext, _rng, _dtp, new TekReleaseWorkflowTime(workflowConfig.Object), luhnModNConfig, luhnModNGenerator, new NullLogger<GenerateTeksCommand>()).ExecuteAsync(new GenerateTeksCommandArgs { TekCountPerWorkflow = 1, WorkflowCount = 1 });

            Assert.Equal(1, _workflowContext.TemporaryExposureKeys.Count());
            Assert.Equal(0, _diagnosisKeysContext.DiagnosisKeys.Count());

            // Act
            await _snapshot.ExecuteAsync(); //Too soon to publish TEKs
            await _eksJob.ExecuteAsync();
            await _manifestJob.ExecuteAsync();

            // Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ManifestV4));
            Assert.Equal(0, _contentDbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySetV2)); // Stuffing not added
        }
    }
}
