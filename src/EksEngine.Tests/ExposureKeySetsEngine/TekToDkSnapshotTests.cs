using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    public abstract class TekToDkSnapshotTests : IDisposable
    {
        #region Implementation

        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbProvider;
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;
        private readonly IWrappedEfExtensions _EfExtensions;
        private readonly LoggerFactory _Lf;
        private readonly Mock<IUtcDateTimeProvider> _DateTimeProvider;
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _OutboundCountries;
        protected TekToDkSnapshotTests(IDbProvider<WorkflowDbContext> workflowFac, IDbProvider<DkSourceDbContext> dkSourceFac, IWrappedEfExtensions efExtensions)
        {
            _WorkflowDbProvider = workflowFac ?? throw new ArgumentNullException(nameof(workflowFac));
            _DkSourceDbProvider = dkSourceFac ?? throw new ArgumentNullException(nameof(dkSourceFac));
            _EfExtensions = efExtensions ?? throw new ArgumentNullException(nameof(efExtensions));
            _DateTimeProvider = new Mock<IUtcDateTimeProvider>();
            _OutboundCountries = new Mock<IOutboundFixedCountriesOfInterestSetting>(MockBehavior.Strict);
            _Lf = new LoggerFactory();
        }

        private SnapshotWorkflowTeksToDksCommand Create()
        {
            _OutboundCountries.Setup(x => x.CountriesOfInterest).Returns(new[] { "GB" });

            return new SnapshotWorkflowTeksToDksCommand(_Lf.CreateLogger<SnapshotWorkflowTeksToDksCommand>(),
                _DateTimeProvider.Object,
                new TransmissionRiskLevelCalculationMk2(),
                _WorkflowDbProvider.CreateNew(),
                _WorkflowDbProvider.CreateNew,
                _DkSourceDbProvider.CreateNew,
                _EfExtensions,
                new IDiagnosticKeyProcessor[] {
                    new ExcludeTrlNoneDiagnosticKeyProcessor(),
                    new FixedCountriesOfInterestOutboundDiagnosticKeyProcessor(_OutboundCountries.Object),
                    new NlToEfgsDsosDiagnosticKeyProcessorMk1()
                });
        }

        private void Write(TekReleaseWorkflowStateEntity[] workflows)
        {
            var db = _WorkflowDbProvider.CreateNew();
            db.KeyReleaseWorkflowStates.AddRange(workflows);
            db.TemporaryExposureKeys.AddRange(workflows.SelectMany(x => x.Teks));
            db.SaveChanges();
            Assert.Equal(workflows.Length, db.KeyReleaseWorkflowStates.Count());
            Assert.Equal(workflows.Sum(x => x.Teks.Count) , db.TemporaryExposureKeys.Count());
        }

        private void GenerateWorkflowTeks(int wfCount, int tekPerWfCount)
        {
            Write(Enumerable.Range(0, wfCount).Select(x => GenWorkflow(x, GenTeks(tekPerWfCount))).ToArray());
        }

        private TekEntity[] GenTeks(int tekPerWfCount)
        {
            var t = _DateTimeProvider.Object.Snapshot;
            return Enumerable.Range(0, tekPerWfCount).Select(x =>
                new TekEntity { 
                    RollingStartNumber = t.AddDays(-x).ToUniversalTime().Date.ToRollingStartNumber(),
                    RollingPeriod = 2, //Corrected by a processor.
                    KeyData = new byte[UniversalConstants.DailyKeyDataByteCount], 
                    PublishAfter = t.AddHours(2) 
                }
            ).ToArray();
        }

        private TekReleaseWorkflowStateEntity GenWorkflow(int key, params TekEntity[] items)
        {
            var now = _DateTimeProvider.Object.Snapshot;

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

        public void Dispose()
        {
            _WorkflowDbProvider.Dispose();
            _DkSourceDbProvider.Dispose();
            _Lf.Dispose();
        }

        #endregion

        [InlineData(0, 0, 120, 0)] //Null case
        [InlineData(1, 10, 119, 0)] //Just before
        [InlineData(1, 10, 120, 10)] //Exactly
        [InlineData(1, 10, 121, 10)] //After
        [Theory]
        [ExclusivelyUses(nameof(TekToDkSnapshotTests))]
        public async Task PublishAfter(int wfCount, int tekPerWfCount, int addMins, int resultCount)
        {
            var t = new DateTime(2020, 11, 5, 12, 0, 0, DateTimeKind.Utc);
            _DateTimeProvider.Setup(x => x.Snapshot).Returns(t);
            var tekCount = wfCount * tekPerWfCount;
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            Assert.Equal(tekCount, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            _DateTimeProvider.Setup(x => x.Snapshot).Returns(t.AddMinutes(addMins));
            var c = Create();
            var result = await c.ExecuteAsync();

            Assert.Equal(resultCount, result.TekReadCount);
            Assert.Equal(tekCount - resultCount, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(result.DkCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.Local.TransmissionRiskLevel != TransmissionRiskLevel.None));
        }

        [InlineData(0, 0)] //Null case
        [InlineData(1, 1)]
        [InlineData(1, 4)] //was 10 - 6 are now filtered out as they have TRL None
        [Theory]
        [ExclusivelyUses(nameof(TekToDkSnapshotTests))]
        public async Task SecondRunShouldChangeNothing(int wfCount, int tekPerWfCount)
        {
            _DateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 14, 00, 0, DateTimeKind.Utc));
            GenerateWorkflowTeks(wfCount, tekPerWfCount);

            //Two hours later
            _DateTimeProvider.Setup(x => x.Snapshot).Returns(new DateTime(2020, 11, 5, 16, 00, 0, DateTimeKind.Utc));
            var tekCount = wfCount * tekPerWfCount;
            Assert.Equal(tekCount, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished));
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.True(_DkSourceDbProvider.CreateNew().DiagnosisKeys.All(x => x.DailyKey.RollingPeriod == UniversalConstants.RollingPeriodRange.Hi)); //Compatible with Apple API

            var result = await Create().ExecuteAsync();
            Assert.Equal(tekCount, result.TekReadCount);
            Assert.Equal(tekCount, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(tekCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());

            //Second run
            result = await Create().ExecuteAsync();

            //No changes
            Assert.Equal(0, result.TekReadCount);
            Assert.Equal(tekCount, _WorkflowDbProvider.CreateNew().TemporaryExposureKeys.Count(x => x.PublishingState != PublishingState.Unpublished));
            Assert.Equal(tekCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
        }
    }
}