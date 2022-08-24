// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Manifest;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Manifest
{
    public abstract class RemoveExpiredManifestsTest
    {
        private readonly ContentDbContext _contentDbContext;
        private Mock<IManifestConfig> _manifestConfigMock;

        protected RemoveExpiredManifestsTest(DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _contentDbContext = new ContentDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _contentDbContext.Database.EnsureCreated();
        }

        [Theory]
        [InlineData(ContentTypes.Manifest)]
        public async Task Remove_Expired_Manifest_By_Type_Should_Leave_One(ContentTypes manifestTypeName)
        {
            //Arrange
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());

            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            CreateManifest();

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            await sut.ExecuteAsync();

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);
            var appConfig = content.Where(x => x.Type == ContentTypes.AppConfig);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 1, $"More than 1 {manifestTypeName} remains after deletion.");

            Assert.NotNull(appConfig);
            Assert.True(appConfig.Count() != 0, $"No AppConfigV2 remains after deletion.");
        }

        [Fact]
        public async Task Remove_Expired_Manifests_Should_Leave_One()
        {
            //Arrange
            _contentDbContext.BulkDelete(_contentDbContext.Content.ToList());

            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            CreateManifest();

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            await sut.ExecuteAsync();

            var content = GetAllContent();

            //Assert
            var manifests = content.Where(x => x.Type == ContentTypes.Manifest);
            var appConfig = content.Where(x => x.Type == ContentTypes.AppConfig);

            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 1, $"More than 1 {ContentTypes.Manifest} remains after deletion.");

            Assert.NotNull(appConfig);
            Assert.True(appConfig.Count() != 0, $"No AppConfigV2 remains after deletion.");
        }

        [Theory]
        [InlineData(ContentTypes.Manifest)]
        public async Task Remove_Zero_Manifest_Should_Not_Crash(ContentTypes manifestTypeName)
        {
            //Arrange
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());

            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            await sut.ExecuteAsync();

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(!manifests.Any(), $"More than 0 {manifestTypeName} remains after deletion");
        }

        private RemoveExpiredManifestsCommand CompileRemoveExpiredManifestsCommand()
        {
            return new RemoveExpiredManifestsCommand(
                _contentDbContext,
                _manifestConfigMock.Object,
                new StandardUtcDateTimeProvider(),
                new NullLogger<RemoveExpiredManifestsCommand>());
        }

        private void CreateManifest()
        {
            var manifestContent = "This is a Manifest";
            var appConfigContent = "This is an AppConfig";

            var today = DateTime.UtcNow;
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var tomorrow = DateTime.UtcNow.AddDays(1);

            _contentDbContext.Content.AddRange(new[]
            {
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(appConfigContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.AppConfig,
                    Created = twoDaysAgo,
                    Release = twoDaysAgo

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.Manifest,
                    Created = twoDaysAgo,
                    Release = twoDaysAgo

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.Manifest,
                    Created = yesterday,
                    Release = yesterday

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.Manifest,
                    Created = today,
                    Release = today

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.Manifest,
                    Created = tomorrow,
                    Release = tomorrow

                },
            });

            _contentDbContext.SaveChanges();
        }

        private IEnumerable<ContentEntity> GetAllContent()
        {
            return _contentDbContext.Set<ContentEntity>();
        }
    }
}
