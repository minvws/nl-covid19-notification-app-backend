// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
{
    public abstract class ManifestV3CreationTest
    {

        private readonly DbContextOptions<ContentDbContext> _contentDbContextOptions;

        protected ManifestV3CreationTest(DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _contentDbContextOptions = contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions));
        }

        [Fact]
        [ExclusivelyUses(nameof(ManifestV3CreationTest))]
        public void ManifestUpdateCommand_ExecuteForV3()
        {
            //Arrange
            PopulateContentDb();

            var sut = CompileManifestUpdateCommand();

            //Act
            sut.ExecuteV3Async().GetAwaiter().GetResult();

            var contentDbContext = new ContentDbContext(_contentDbContextOptions);
            contentDbContext.Database.EnsureCreated();

            var result = contentDbContext.SafeGetLatestContentAsync(ContentTypes.ManifestV3, DateTime.Now).GetAwaiter().GetResult();

            //Assert
            Assert.NotNull(result);

            using var zipFileInMemory = new MemoryStream();
            zipFileInMemory.Write(result.Content, 0, result.Content.Length);
            using (var zipFileContent = new ZipArchive(zipFileInMemory, ZipArchiveMode.Read, false))
            {
                var manifestContent = Encoding.ASCII.GetString(zipFileContent.ReadEntry(ZippedContentEntryNames.Content));
                var correctResLocation = manifestContent.IndexOf("TheV3ResourceBundleId");
                var wrongResLocation = manifestContent.IndexOf("TheWrongResourceBundleId");
                Assert.True(correctResLocation > 0);
                Assert.True(wrongResLocation == -1);
            }
        }

        private ManifestUpdateCommand CompileManifestUpdateCommand()
        {
            var eksConfigMock = new Mock<IEksConfig>();
            var lf = new LoggerFactory();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var jsonSerialiser = new StandardJsonSerializer();
            var entityFormatterMock = new Mock<IContentEntityFormatter>();

            Func<IContentEntityFormatter> formatterForV3 = () =>
                new StandardContentEntityFormatter(
                    new ZippedSignedContentFormatter(
                    TestSignerHelpers.CreateCmsSignerEnhanced(lf)),
                    new Sha256HexPublishingIdService(),
                    jsonSerialiser
                        );

            var result = new ManifestUpdateCommand(
                new ManifestV2Builder(
                    new ContentDbContext(_contentDbContextOptions),
                    eksConfigMock.Object,
                    dateTimeProvider),
                new ManifestV3Builder(
                    new ContentDbContext(_contentDbContextOptions),
                    eksConfigMock.Object,
                    dateTimeProvider),
                new ManifestV4Builder(
                    new ContentDbContext(_contentDbContextOptions),
                    eksConfigMock.Object,
                    dateTimeProvider),
                new ContentDbContext(_contentDbContextOptions),
                new ManifestUpdateCommandLoggingExtensions(lf.CreateLogger<ManifestUpdateCommandLoggingExtensions>()),
                dateTimeProvider,
                jsonSerialiser,
                formatterForV3
                );

            return result;
        }

        private void PopulateContentDb()
        {
            var contentDbContext = new ContentDbContext(_contentDbContextOptions);
            contentDbContext.Database.EnsureCreated();

            var yesterday = DateTime.Now.AddDays(-1);
            var content = "This is a ResourceBundleV3";

            contentDbContext.Content.AddRange(new[]
            {
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = "TheV3ResourceBundleId",
                    ContentTypeName = "Mwah",
                    Type = ContentTypes.ResourceBundleV3,
                    Created = yesterday,
                    Release = yesterday

                },
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = "TheWrongResourceBundleId",
                    ContentTypeName = "Mwah",
                    Type = ContentTypes.ResourceBundleV2,
                    Created = yesterday,
                    Release = yesterday

                }
            });

            contentDbContext.SaveChanges();
        }
    }
}
