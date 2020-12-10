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

        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;
        private readonly IDbProvider<IksPublishingJobDbContext> _IksPublishingDbProvider;
        private readonly LoggerFactory _Lf = new LoggerFactory();
        private readonly Mock<IUtcDateTimeProvider> _UtcDateTimeProviderMock = new Mock<IUtcDateTimeProvider>();
        private readonly Mock<IIksConfig> _IksConfigMock = new Mock<IIksConfig>();

        protected MarkDiagnosisKeysAsUsedByIksTests(IDbProvider<DkSourceDbContext> dkSourceDbProvider, IDbProvider<IksPublishingJobDbContext> iksPublishingDbProvider)
        {
            _DkSourceDbProvider = dkSourceDbProvider;
            _IksPublishingDbProvider = iksPublishingDbProvider;
        }

        private MarkDiagnosisKeysAsUsedByIks Create()
        {
            return new MarkDiagnosisKeysAsUsedByIks(
                _DkSourceDbProvider.CreateNew,
                _IksConfigMock.Object,
                _IksPublishingDbProvider.CreateNew,
                _Lf.CreateLogger<MarkDiagnosisKeysAsUsedByIks>()
            );
        }

        public void Dispose()
        {
            _DkSourceDbProvider.Dispose();
            _IksPublishingDbProvider.Dispose();
            _Lf.Dispose();
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
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _IksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_DkSourceDbProvider);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_IksPublishingDbProvider);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _IksPublishingDbProvider.CreateNew().Input.Count());
            Assert.Equal(baseCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(0, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var result = await Create().ExecuteAsync();

            Assert.Equal(baseCount, result.Count);
            Assert.Equal(baseCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(baseCount, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
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
            _UtcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _IksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_DkSourceDbProvider);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));
            gen.Write(gen.AlreadyPublished(gen.GenerateDks(baseCount)));
            gen.Write(gen.NotLocal(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_IksPublishingDbProvider);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _IksPublishingDbProvider.CreateNew().Input.Count());
            Assert.Equal(baseCount * 3, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count());
            Assert.Equal(baseCount * 2, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var c = Create();
            var result = await c.ExecuteAsync();

            Assert.Equal(baseCount, result.Count); //Affected
            Assert.Equal(baseCount, _IksPublishingDbProvider.CreateNew().Input.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _DkSourceDbProvider.CreateNew().DiagnosisKeys.Count(x => x.PublishedToEfgs)); //
        }
    }
}