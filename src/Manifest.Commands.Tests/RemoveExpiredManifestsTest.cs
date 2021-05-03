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
        private readonly IDbProvider<ContentDbContext> _ContentDbProvider;
        private Mock<IManifestConfig> _ManifestConfigMock;

        public RemoveExpiredManifestsTest(IDbProvider<ContentDbContext> contentDbProvider)
        {
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
        }

        [Theory]
        [InlineData(ContentTypes.ManifestV2)]
        [InlineData(ContentTypes.ManifestV3)]
        [InlineData(ContentTypes.ManifestV4)]
        public void Remove_Expired_Manifest_Should_Leave_One(string manifestTypeName)
        {
            //Arrange
            _ManifestConfigMock = new Mock<IManifestConfig>();
            _ManifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            CreateManifestsForManifestType(manifestTypeName);

            //Act
            RemoveExpiredManifestsExecutePerManifestType(manifestTypeName);

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);
            var appConfig = content.Where(x => x.Type == ContentTypes.AppConfigV2);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 1, $"More than 1 {manifestTypeName} remains after deletion.");

            Assert.NotNull(appConfig);
            Assert.True(appConfig.Count() != 0, $"No AppConfigV2 remains after deletion.");
        }

        [Theory]
        [InlineData(ContentTypes.ManifestV2)]
        [InlineData(ContentTypes.ManifestV3)]
        [InlineData(ContentTypes.ManifestV4)]
        public void Remove_Zero_Manifest_Should_Not_Crash(string manifestTypeName)
        {
            //Arrange
            _ManifestConfigMock = new Mock<IManifestConfig>();
            _ManifestConfigMock.Setup(x => x.KeepAliveCount).Returns(1);

            //Act
            RemoveExpiredManifestsExecutePerManifestType(manifestTypeName);

            var content = GetAllContent();

            var manifests = content.Where(x => x.Type == manifestTypeName);

            //Assert
            Assert.NotNull(manifests);
            Assert.True(manifests.Count() == 0, $"More than 0 {manifestTypeName} remains after deletion.");
        }

        private void RemoveExpiredManifestsExecutePerManifestType(string manifestTypeName)
        {
            switch (manifestTypeName)
            {
                case ContentTypes.Manifest:
                    var sut = CompileRemoveExpiredManifestsCommand();
                    sut.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                case ContentTypes.ManifestV2:
                    var sutV2 = CompileRemoveExpiredManifestsV2Command();
                    sutV2.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                case ContentTypes.ManifestV3:
                    var sutV3 = CompileRemoveExpiredManifestsV3Command();
                    sutV3.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                case ContentTypes.ManifestV4:
                    var sutV4 = CompileRemoveExpiredManifestsV4Command();
                    sutV4.ExecuteAsync().GetAwaiter().GetResult();
                    break;
                default:
                    Assert.True(false, $"No {manifestTypeName} remove command exists.");
                    break;
            }
        }

        private RemoveExpiredManifestsCommand CompileRemoveExpiredManifestsCommand()
        {
            var logger = new ExpiredManifestLoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestLoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsCommand(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private RemoveExpiredManifestsV2Command CompileRemoveExpiredManifestsV2Command()
        {
            var logger = new ExpiredManifestV2LoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestV2LoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsV2Command(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private RemoveExpiredManifestsV3Command CompileRemoveExpiredManifestsV3Command()
        {
            var logger = new ExpiredManifestV3LoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestV3LoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsV3Command(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private RemoveExpiredManifestsV4Command CompileRemoveExpiredManifestsV4Command()
        {
            var logger = new ExpiredManifestV4LoggingExtensions(new LoggerFactory().CreateLogger<ExpiredManifestV4LoggingExtensions>());
            var dateTimeProvider = new StandardUtcDateTimeProvider();

            var result = new RemoveExpiredManifestsV4Command(_ContentDbProvider.CreateNew, logger, _ManifestConfigMock.Object, dateTimeProvider);

            return result;
        }

        private void CreateManifestsForManifestType(string manifestTypeName)
        {
            var database = _ContentDbProvider.CreateNew();

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
            var database = _ContentDbProvider.CreateNew();
            return database.Set<ContentEntity>();
        }

        public void Dispose()
        {
            _ContentDbProvider.Dispose();
        }
    }
}