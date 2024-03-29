// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Tests
{
    public abstract class ManifestUpdateCommandTest
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ManifestUpdateCommand _sut;
        private readonly Mock<IUtcDateTimeProvider> _dateTimeProviderMock;
        private readonly DateTime _mockedTime = DateTime.UtcNow;

        protected ManifestUpdateCommandTest(DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _contentDbContext = new ContentDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _contentDbContext.Database.EnsureCreated();

            var hsmSignerServiceMock = new Mock<IHsmSignerService>();
            hsmSignerServiceMock.Setup(x => x.GetNlCmsSignatureAsync(new byte[0]))
                .ReturnsAsync(new byte[] { 2 });

            var eksConfigMock = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfigMock.Setup(x => x.LifetimeDays)
                .Returns(14);

            _dateTimeProviderMock = new Mock<IUtcDateTimeProvider>();
            _dateTimeProviderMock.Setup(x => x.Snapshot)
                .Returns(_mockedTime);

            var jsonSerializer = new StandardJsonSerializer();
            var logger = new NullLogger<ManifestUpdateCommand>();

            IContentEntityFormatter contentFormatterInjector = new StandardContentEntityFormatter(
                    new ZippedSignedContentFormatter(hsmSignerServiceMock.Object),
                    jsonSerializer);

            _sut = new ManifestUpdateCommand(
                new ManifestBuilder(_contentDbContext, eksConfigMock.Object, _dateTimeProviderMock.Object),
                _contentDbContext,
                logger,
                _dateTimeProviderMock.Object,
                jsonSerializer,
                contentFormatterInjector
            );
        }

        [Fact]
        public async Task NoManifestInDb_Execute_ManifestInDb()
        {
            // Arrange
            //Act
            await _sut.ExecuteAsync();

            //Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.Manifest));
        }

        [Fact]
        public async Task NoManifestInDb_Execute_ManifestsInDb()
        {
            // Arrange
            //Act
            await _sut.ExecuteAsync();

            //Advance the clock
            _dateTimeProviderMock.Setup(x => x.Snapshot)
                .Returns(_mockedTime.AddMinutes(2));

            await _sut.ExecuteAsync();

            //Assert
            Assert.Equal(1, _contentDbContext.Content.Count(x => x.Type == ContentTypes.Manifest));
        }
    }
}
