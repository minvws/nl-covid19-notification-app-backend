// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    public abstract class RemoveExpiredManifestsTest : IDisposable
    {
        private readonly IDbProvider<ContentDbContext> _contentDbProvider;
        private Mock<IManifestConfig> _manifestConfigMock;

        private readonly List<string> _manifestTypes = new List<string>
        {
            ContentTypes.Manifest,
            ContentTypes.ManifestV2,
            ContentTypes.ManifestV3,
            ContentTypes.ManifestV4
        };

        public RemoveExpiredManifestsTest(IDbProvider<ContentDbContext> contentDbProvider)
        {
            _contentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
        }

        [Theory]
        [InlineData(ContentTypes.ManifestV2)]
        [InlineData(ContentTypes.ManifestV3)]
        [InlineData(ContentTypes.ManifestV4)]
        public void Remove_Expired_Manifest_By_Type_Should_Leave_One(string manifestTypeName)
        {
            //Arrange
            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            CreateManifestsForManifestType(manifestTypeName);

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            sut.Execute();

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);
            var appConfig = content.Where(x => x.Type == ContentTypes.AppConfigV2);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 1, $"More than 1 {manifestTypeName} remains after deletion.");

            Assert.NotNull(appConfig);
            Assert.True(appConfig.Count() != 0, $"No AppConfigV2 remains after deletion.");
        }

        [Fact]
        public void Remove_Expired_Manifests_Should_Leave_One_Per_Type()
        {
            //Arrange
            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            foreach (var manifestType in _manifestTypes)
            {
                CreateManifestsForManifestType(manifestType);
            }

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            sut.Execute();

            var content = GetAllContent();

            //Assert per type
            foreach (var manifestType in _manifestTypes)
            {
                var manifests = content.Where(x => x.Type == manifestType);
                var appConfig = content.Where(x => x.Type == ContentTypes.AppConfigV2);


                Assert.NotNull(manifests);
                Assert.True(manifests.Count() == 1, $"More than 1 {manifestType} remains after deletion.");

                Assert.NotNull(appConfig);
                Assert.True(appConfig.Count() != 0, $"No AppConfigV2 remains after deletion.");
            }
        }

        [Theory]
        [InlineData(ContentTypes.ManifestV2)]
        [InlineData(ContentTypes.ManifestV3)]
        [InlineData(ContentTypes.ManifestV4)]
        public void Remove_Zero_Manifest_By_Type_Should_Not_Crash(string manifestTypeName)
        {
            //Arrange
            _manifestConfigMock = new Mock<IManifestConfig>();
            _manifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            //Act
            var sut = CompileRemoveExpiredManifestsCommand();
            sut.Execute();

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 0, $"More than 0 {manifestTypeName} remains after deletion.");
        }

        private RemoveExpiredManifestsCommand CompileRemoveExpiredManifestsCommand()
        {
            var loggerMock = new Mock<ILogger<RemoveExpiredManifestsReceiver>>();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var receiver = new RemoveExpiredManifestsReceiver(_contentDbProvider.CreateNew, _manifestConfigMock.Object, dateTimeProvider, loggerMock.Object);

            var result = new RemoveExpiredManifestsCommand(receiver);
            return result;
        }

        private void CreateManifestsForManifestType(string manifestTypeName)
        {
            var database = _contentDbProvider.CreateNew();

            var manifestContent = "This is a Manifest";
            var appConfigContent = "This is an AppConfig";

            var today = DateTime.UtcNow;
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var tomorrow = DateTime.UtcNow.AddDays(1);

            database.Content.AddRange(new[]
            {
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(appConfigContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = ContentTypes.AppConfigV2,
                    Created = twoDaysAgo,
                    Release = twoDaysAgo

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = twoDaysAgo,
                    Release = twoDaysAgo

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = yesterday,
                    Release = yesterday

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = today,
                    Release = today

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(manifestContent),
                    PublishingId = It.IsAny<string>(),
                    ContentTypeName = It.IsAny<string>(),
                    Type = manifestTypeName,
                    Created = tomorrow,
                    Release = tomorrow

                },
            });

            database.SaveChanges();
        }

        private IEnumerable<ContentEntity> GetAllContent()
        {
            var database = _contentDbProvider.CreateNew();
            return database.Set<ContentEntity>();
        }

        public void Dispose()
        {
            _contentDbProvider.Dispose();
        }
    }
}
