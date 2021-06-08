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
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop.TestData;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    public abstract class MarkDiagnosisKeysAsUsedByIksTests : IDisposable
    {
        #region Implementation

        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbProvider;
        private readonly IDbProvider<IksPublishingJobDbContext> _iksPublishingDbProvider;
        private readonly LoggerFactory _lf = new LoggerFactory();
        private readonly Mock<IUtcDateTimeProvider> _utcDateTimeProviderMock = new Mock<IUtcDateTimeProvider>();
        private readonly Mock<IIksConfig> _iksConfigMock = new Mock<IIksConfig>();

        protected MarkDiagnosisKeysAsUsedByIksTests(IDbProvider<DkSourceDbContext> dkSourceDbProvider, IDbProvider<IksPublishingJobDbContext> iksPublishingDbProvider)
        {
            _dkSourceDbProvider = dkSourceDbProvider;
            _iksPublishingDbProvider = iksPublishingDbProvider;
        }

        private MarkDiagnosisKeysAsUsedByIks Create()
        {
            return new MarkDiagnosisKeysAsUsedByIks(
                _dkSourceDbProvider.CreateNew,
                _iksConfigMock.Object,
                _iksPublishingDbProvider.CreateNew,
                _lf.CreateLogger<MarkDiagnosisKeysAsUsedByIks>()
            );
        }

        public void Dispose()
        {
            _dkSourceDbProvider.Dispose();
            _iksPublishingDbProvider.Dispose();
            _lf.Dispose();
        }

        #endregion

        //Not pub'd x 1
        //Already Pub = 0
        //Efgs x 0
        [InlineData(0)] //Null case
        [InlineData(100)]
        [Theory]
        [ExclusivelyUses(nameof(MarkDiagnosisKeysAsUsedByIksTests))]
        public async Task Simple(int baseCount)
        {
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _iksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_dkSourceDbProvider);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_iksPublishingDbProvider);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _iksPublishingDbProvider.CreateNew().Input.Count());
            Assert.Equal(baseCount, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var result = await Create().ExecuteAsync();

            Assert.Equal(baseCount, result.Count);
            Assert.Equal(baseCount, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(baseCount, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
        }

        //Not pub'd x 1
        //Already Pub = 1
        //Efgs x 1
        [InlineData(0)] //Null case
        [InlineData(100)]
        [Theory]
        [ExclusivelyUses(nameof(MarkDiagnosisKeysAsUsedByIksTests))]
        public async Task RealisticBothSourcesOfDks(int baseCount)
        {
            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _iksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_dkSourceDbProvider);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));
            gen.Write(gen.AlreadyPublished(gen.GenerateDks(baseCount)));
            gen.Write(gen.NotLocal(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_iksPublishingDbProvider);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _iksPublishingDbProvider.CreateNew().Input.Count());
            Assert.Equal(baseCount * 3, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(baseCount * 2, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var c = Create();
            var result = await c.ExecuteAsync();

            Assert.Equal(baseCount, result.Count); //Affected
            Assert.Equal(baseCount, _iksPublishingDbProvider.CreateNew().Input.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _dkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs)); //
        }
    }
}
