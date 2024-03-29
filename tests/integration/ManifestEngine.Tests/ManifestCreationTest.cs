// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Tests
{
    public abstract class ManifestCreationTest
    {
        private readonly ContentDbContext _contentDbContext;

        protected ManifestCreationTest(DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _contentDbContext = new ContentDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _contentDbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task ManifestUpdateCommand_Execute()
        {
            //Arrange
            await BulkDeleteAllDataInTest();
            PopulateContentDb();

            var sut = CompileManifestUpdateCommand();

            //Act
            await sut.ExecuteAsync();

            var result = _contentDbContext.SafeGetLatestContentAsync(ContentTypes.Manifest, DateTime.UtcNow).GetAwaiter().GetResult();

            //Assert
            Assert.NotNull(result);

            await using var zipFileInMemory = new MemoryStream();
            zipFileInMemory.Write(result.Content, 0, result.Content.Length);
            using var zipFileContent = new ZipArchive(zipFileInMemory, ZipArchiveMode.Read, false);
            var manifestContent = Encoding.ASCII.GetString(zipFileContent.ReadEntry(ZippedContentEntryNames.Content));
            var correctResLocation = manifestContent.IndexOf("CorrectId", StringComparison.Ordinal);
            Assert.True(correctResLocation > 0);
        }

        private ManifestUpdateCommand CompileManifestUpdateCommand()
        {
            var eksConfigMock = new Mock<IEksConfig>();
            eksConfigMock.SetupGet(x => x.LifetimeDays).Returns(14);
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var jsonSerialiser = new StandardJsonSerializer();

            IContentEntityFormatter formatterForV3 = new StandardContentEntityFormatter(
                    new ZippedSignedContentFormatter(TestSignerHelpers.CreateHsmSignerService()),
                    jsonSerialiser);

            var result = new ManifestUpdateCommand(
                new ManifestBuilder(
                    _contentDbContext,
                    eksConfigMock.Object,
                    dateTimeProvider),
                _contentDbContext,
                new NullLogger<ManifestUpdateCommand>(),
                dateTimeProvider,
                jsonSerialiser,
                formatterForV3
                );

            return result;
        }

        private void PopulateContentDb()
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var content = "This is a test";

            _contentDbContext.Content.AddRange(new[]
            {
                new ContentEntity{
                    Content = Encoding.ASCII.GetBytes(content),
                    PublishingId = "CorrectId",
                    ContentTypeName = "ExposureKeySet",
                    Type = ContentTypes.ExposureKeySet,
                    Created = yesterday,
                    Release = yesterday

                }
            });

            _contentDbContext.SaveChanges();
        }

        private async Task BulkDeleteAllDataInTest()
        {
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());
        }
    }
}
