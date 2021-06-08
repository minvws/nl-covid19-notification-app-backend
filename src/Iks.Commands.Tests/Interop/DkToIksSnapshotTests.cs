// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    public abstract class DkToIksSnapshotTests : IDisposable
    {
        #region Implementation

        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbProvider;
        private readonly IDbProvider<IksPublishingJobDbContext> _iksPublishingDbProvider;
        private readonly LoggerFactory _lf = new LoggerFactory();
        private readonly Mock<IUtcDateTimeProvider> _dateTimeProvider = new Mock<IUtcDateTimeProvider>();
        private readonly Mock<IOutboundFixedCountriesOfInterestSetting> _countriesConfigMock = new Mock<IOutboundFixedCountriesOfInterestSetting>(MockBehavior.Strict);

        protected DkToIksSnapshotTests(IDbProvider<DkSourceDbContext> dkSourceDbProvider, IDbProvider<IksPublishingJobDbContext> iksPublishingDbProvider)
        {
            _dkSourceDbProvider = dkSourceDbProvider;
            _iksPublishingDbProvider = iksPublishingDbProvider;
        }

        private IksInputSnapshotCommand Create()
        {
            _countriesConfigMock.Setup(x => x.CountriesOfInterest).Returns(new[] { "DE", "BG" });


            return new IksInputSnapshotCommand(_lf.CreateLogger<IksInputSnapshotCommand>(),
                _dkSourceDbProvider.CreateNew(),
                _iksPublishingDbProvider.CreateNew,
                _countriesConfigMock.Object
            );
        }

        private void Write(DiagnosisKeyEntity[] items)
        {
            var db = _dkSourceDbProvider.CreateNew();
            db.DiagnosisKeys.AddRange(items);
            db.SaveChanges();
        }

        private DiagnosisKeyEntity[] AlreadyPublished(DiagnosisKeyEntity[] items)
        {
            foreach (var i in items)
            {
                i.Origin = TekOrigin.Local;
                i.PublishedToEfgs = true;
                i.PublishedLocally = true;
            }

            return items;
        }

        private DiagnosisKeyEntity[] LocalDksForLocalPeople(DiagnosisKeyEntity[] items)
        {
            foreach (var i in items)
            {
                i.Origin = TekOrigin.Local;
                i.PublishedLocally = true;
                i.Local = new LocalTekInfo
                {
                    //DaysSinceSymptomsOnset = 4,
                    //TransmissionRiskLevel = TransmissionRiskLevel.High,
                };
                i.Efgs = new EfgsTekInfo
                {
                    CountriesOfInterest = null,
                    ReportType = ReportType.ConfirmedTest,
                    DaysSinceSymptomsOnset = 0,
                    CountryOfOrigin = "DE"
                };
            }

            return items;
        }

        private DiagnosisKeyEntity[] NotLocal(DiagnosisKeyEntity[] items)
        {
            foreach (var i in items)
            {
                i.Origin = TekOrigin.Efgs;
                i.PublishedToEfgs = true;
                i.Efgs = new EfgsTekInfo
                {
                    DaysSinceSymptomsOnset = 0,
                    CountriesOfInterest = "NL",
                    CountryOfOrigin = "DE",
                    ReportType = ReportType.Recursive
                };
                i.Local = new LocalTekInfo
                {
                };
            }

            return items;
        }

        private DiagnosisKeyEntity[] GenerateDks(int count)
        {
            var t = _dateTimeProvider.Object.Snapshot;
            return Enumerable.Range(0, count).Select(x =>
                new DiagnosisKeyEntity
                {
                    DailyKey = new DailyKey
                    {
                        RollingStartNumber = t.AddDays(-x % 14).ToUniversalTime().Date.ToRollingStartNumber(),
                        RollingPeriod = 144, //Already corrected in DKs
                        KeyData = new byte[UniversalConstants.DailyKeyDataByteCount],
                    },
                    Local = new LocalTekInfo
                    {
                        DaysSinceSymptomsOnset = -2
                    }
                }).ToArray();
        }

        private void Setup(int baseCount)
        {
            Write(LocalDksForLocalPeople(GenerateDks(baseCount * 3)));
            Write(AlreadyPublished(GenerateDks(baseCount)));
            Write(NotLocal(GenerateDks(baseCount)));
        }

        public void Dispose()
        {
            _iksPublishingDbProvider.Dispose();
            _dkSourceDbProvider.Dispose();
            _lf.Dispose();
        }

        #endregion

        [ExclusivelyUses(nameof(DkToIksSnapshotTests))]
        [InlineData(0)] //Null case
        [InlineData(100)]
        [Theory]
        public async Task Run1(int baseCount)
        {
            _dateTimeProvider.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            Setup(baseCount);

            Assert.Equal(baseCount * 5, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _iksPublishingDbProvider.CreateNew().Input.Count());

            var c = Create();
            var result = await c.ExecuteAsync();

            Assert.Equal(3 * baseCount, result.Count);
            Assert.Equal(result.Count, _iksPublishingDbProvider.CreateNew().Input.Count());
        }
    }
}
