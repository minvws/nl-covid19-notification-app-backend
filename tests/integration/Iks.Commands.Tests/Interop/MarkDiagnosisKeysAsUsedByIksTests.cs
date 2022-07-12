// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop.TestData;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop
{
    public abstract class MarkDiagnosisKeysAsUsedByIksTests
    {
        private readonly DiagnosisKeysDbContext _diagnosisKeysDbContext;
        private readonly IksPublishingJobDbContext _iksPublishingDbContext;
        private readonly Mock<IUtcDateTimeProvider> _utcDateTimeProviderMock = new Mock<IUtcDateTimeProvider>();
        private readonly Mock<IIksConfig> _iksConfigMock = new Mock<IIksConfig>();

        protected MarkDiagnosisKeysAsUsedByIksTests(DbContextOptions<DiagnosisKeysDbContext> diagnosisKeysDbContextOptions, DbContextOptions<IksPublishingJobDbContext> iksPublishingJobDbContextOptions)
        {
            _diagnosisKeysDbContext = new DiagnosisKeysDbContext(diagnosisKeysDbContextOptions ?? throw new ArgumentNullException(nameof(diagnosisKeysDbContextOptions)));
            _diagnosisKeysDbContext.Database.EnsureCreated();

            _iksPublishingDbContext = new IksPublishingJobDbContext(iksPublishingJobDbContextOptions ?? throw new ArgumentNullException(nameof(iksPublishingJobDbContextOptions)));
            _iksPublishingDbContext.Database.EnsureCreated();
        }

        private MarkDiagnosisKeysAsUsedByIks Create()
        {
            return new MarkDiagnosisKeysAsUsedByIks(
                _diagnosisKeysDbContext,
                _iksConfigMock.Object,
                _iksPublishingDbContext,
                new NullLogger<MarkDiagnosisKeysAsUsedByIks>()
            );
        }

        //Not pub'd x 1
        //Already Pub = 0
        //Efgs x 0
        [InlineData(0)] //Null case
        [InlineData(100)]
        [Theory]
        public async Task Simple(int baseCount)
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _iksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_diagnosisKeysDbContext);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_iksPublishingDbContext);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _iksPublishingDbContext.Input.Count());
            Assert.Equal(baseCount, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(0, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var result = await Create().ExecuteAsync();

            Assert.Equal(baseCount, result.Count);
            Assert.Equal(baseCount, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));
            Assert.Equal(baseCount, _diagnosisKeysDbContext.DiagnosisKeys.Count());
        }

        //Not pub'd x 1
        //Already Pub = 1
        //Efgs x 1
        [InlineData(0)] //Null case
        [InlineData(100)]
        [Theory]
        public async Task RealisticBothSourcesOfDks(int baseCount)
        {
            // Arrange
            await BulkDeleteAllDataInTest();

            _utcDateTimeProviderMock.Setup(x => x.Snapshot).Returns(DateTime.UtcNow);
            _iksConfigMock.Setup(x => x.PageSize).Returns(10);

            var gen = new DiagnosisKeyTestDataGenerator(_diagnosisKeysDbContext);
            var items = gen.Write(gen.LocalDksForLocalPeople(gen.GenerateDks(baseCount)));
            gen.Write(gen.AlreadyPublished(gen.GenerateDks(baseCount)));
            gen.Write(gen.NotLocal(gen.GenerateDks(baseCount)));

            var gen2 = new IksDkSnapshotTestDataGenerator(_iksPublishingDbContext);
            gen2.Write(gen2.GenerateInput(items));

            Assert.Equal(baseCount, _iksPublishingDbContext.Input.Count());
            Assert.Equal(baseCount * 3, _diagnosisKeysDbContext.DiagnosisKeys.Count());
            Assert.Equal(baseCount * 2, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs));

            var c = Create();
            var result = await c.ExecuteAsync();

            Assert.Equal(baseCount, result.Count); //Affected
            Assert.Equal(baseCount, _iksPublishingDbContext.Input.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _diagnosisKeysDbContext.DiagnosisKeys.Count()); //Unchanged
            Assert.Equal(baseCount * 3, _diagnosisKeysDbContext.DiagnosisKeys.Count(x => x.PublishedToEfgs)); //
        }

        private async Task BulkDeleteAllDataInTest()
        {
            await _iksPublishingDbContext.BulkDeleteAsync(_iksPublishingDbContext.Input.ToList());
            await _iksPublishingDbContext.BulkDeleteAsync(_iksPublishingDbContext.Output.ToList());
            await _diagnosisKeysDbContext.BulkDeleteAsync(_diagnosisKeysDbContext.DiagnosisKeys.ToList());
        }
    }
}
