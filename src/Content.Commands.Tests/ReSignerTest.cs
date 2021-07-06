// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    public abstract class ReSignerTest
    {
        private readonly ContentDbContext _contentDbContext;

        protected ReSignerTest(DbContextOptions<ContentDbContext> contentDbContextOptions)
        {
            _contentDbContext = new ContentDbContext(contentDbContextOptions);
            _contentDbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task ResignManifest()
        {
            // Arrange
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());

            var lf = new LoggerFactory();
            var resignerLogger = new ResignerLoggingExtensions(lf.CreateLogger<ResignerLoggingExtensions>());

            var d = DateTime.Now;

            await using var testContentStream = ResourcesHook.GetManifestResourceStream("Resources.ResignTestManifest.zip");

            await using var m = new MemoryStream();
            await testContentStream.CopyToAsync(m);
            var zipContent = m.ToArray();

            var m1 = new ContentEntity { Content = zipContent, PublishingId = "1", ContentTypeName = "Meh", Type = ContentTypes.Manifest, Created = d, Release = d };

            await _contentDbContext.Content.AddRangeAsync(new[] {
                m1,
                new ContentEntity { Content = new byte[0], PublishingId = "2", ContentTypeName = "Meh", Type = ContentTypes.AppConfig, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "3", ContentTypeName = "Meh", Type = ContentTypes.AppConfigV2, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "4", ContentTypeName = "Meh", Type = ContentTypes.ExposureKeySet, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "5", ContentTypeName = "Meh", Type = ContentTypes.ExposureKeySetV2, Created = d, Release = d },
            });

            await _contentDbContext.SaveChangesAsync();

            var resigner = new NlContentResignCommand(_contentDbContext, TestSignerHelpers.CreateCmsSignerEnhanced(lf), resignerLogger);
            await resigner.ExecuteAsync(ContentTypes.Manifest, ContentTypes.ManifestV2, ZippedContentEntryNames.Content);

            //check the numbers
            Assert.Equal(6, _contentDbContext.Content.Count());

            var m2 = _contentDbContext.Content.Single(x => x.PublishingId == "1" && x.Type == ContentTypes.ManifestV2);

            Assert.Equal(m1.Created, m2.Created);
            Assert.Equal(m1.Release, m2.Release);

            var ms1 = new MemoryStream(zipContent);
            using var zip1 = new ZipArchive(ms1);

            var ms2 = new MemoryStream(m2.Content);
            using var zip2 = new ZipArchive(ms2);

            Assert.True(Enumerable.SequenceEqual(zip1.ReadEntry(ZippedContentEntryNames.Content), zip2.ReadEntry(ZippedContentEntryNames.Content)));
            Assert.NotEqual(zip1.GetEntry(ZippedContentEntryNames.NlSignature), zip2.GetEntry(ZippedContentEntryNames.NlSignature));
        }

        [Fact]
        public async Task Re_sign_all_existing_earlier_content()
        {
            // Arrange
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());

            var lf = new LoggerFactory();
            var resignerLogger = new ResignerLoggingExtensions(lf.CreateLogger<ResignerLoggingExtensions>());

            //Add some db rows to Content
            var d = DateTime.Now;
            var laterDate = d.AddDays(1);
            var publishingId = "1";

            await using var testContentStream = ResourcesHook.GetManifestResourceStream("Resources.ResignAppConfig.zip");
            await using var m = new MemoryStream();
            await testContentStream.CopyToAsync(m);
            var zipContent = m.ToArray();

            //Adding identical content items
            var sourceAppConfigContent1 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d.AddMilliseconds(1), Release = laterDate };
            var sourceAppConfigContent2 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d.AddMilliseconds(2), Release = laterDate };
            var sourceAppConfigContent3 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d.AddMilliseconds(3), Release = laterDate };

            await _contentDbContext.Content.AddRangeAsync(
                sourceAppConfigContent1,
                sourceAppConfigContent2,
                sourceAppConfigContent3
            );

            await _contentDbContext.SaveChangesAsync();

            Assert.Equal(3, _contentDbContext.Content.Count());

            var resigner = new NlContentResignCommand(_contentDbContext, TestSignerHelpers.CreateCmsSignerEnhanced(lf), resignerLogger);
            await resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);

            //check the numbers
            Assert.Equal(6, _contentDbContext.Content.Count());

            var resignedAppConfigContent = _contentDbContext.Content.Where(x => x.PublishingId == publishingId && x.Type == ContentTypes.AppConfigV2);

            var originalContentStream = new MemoryStream(zipContent);
            using var originalZipArchive = new ZipArchive(originalContentStream);
            foreach (var i in resignedAppConfigContent)
            {
                //Created is bumped by a few ms so can't be compared here
                Assert.Equal(sourceAppConfigContent1.Release, i.Release);

                var s = new MemoryStream(i.Content);
                using var z = new ZipArchive(s);

                Assert.True(Enumerable.SequenceEqual(originalZipArchive.ReadEntry(ZippedContentEntryNames.Content), z.ReadEntry(ZippedContentEntryNames.Content)));
                Assert.NotEqual(originalZipArchive.GetEntry(ZippedContentEntryNames.NlSignature), z.GetEntry(ZippedContentEntryNames.NlSignature));
            }

            //Repeating should have no effect
            await resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);
            Assert.Equal(6, _contentDbContext.Content.Count());
        }

        [Fact]
        public async Task Re_sign_content_that_does_not_already_have_an_equivalent_resigned_entry()
        {
            // Arrange
            await _contentDbContext.BulkDeleteAsync(_contentDbContext.Content.ToList());

            var lf = new LoggerFactory();
            var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());
            var resignerLogger = new ResignerLoggingExtensions(lf.CreateLogger<ResignerLoggingExtensions>());

            //Add some db rows to Content
            var d = DateTime.Now;
            var laterDate = d.AddDays(1);
            var publishingId = "1";

            await using var testContentStream = ResourcesHook.GetManifestResourceStream("Resources.ResignAppConfig.zip");
            await using var m = new MemoryStream();
            await testContentStream.CopyToAsync(m);
            var zipContent = m.ToArray();

            //Adding identical content items
            var sourceAppConfigContent1 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d, Release = laterDate };
            var sourceAppConfigContent2 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d, Release = laterDate };
            var sourceAppConfigContent3 = new ContentEntity { Content = zipContent, PublishingId = publishingId, ContentTypeName = ".", Type = ContentTypes.AppConfig, Created = d, Release = laterDate };

            await _contentDbContext.Content.AddRangeAsync(
                sourceAppConfigContent1,
                sourceAppConfigContent2,
                sourceAppConfigContent3
            );

            await _contentDbContext.SaveChangesAsync();

            Assert.Equal(3, _contentDbContext.Content.Count());

            var resigner = new NlContentResignCommand(_contentDbContext, TestSignerHelpers.CreateCmsSignerEnhanced(lf), resignerLogger);
            await resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);

            //check the numbers
            Assert.Equal(4, _contentDbContext.Content.Count());

            var resignedAppConfigContent = _contentDbContext.Content.Where(x => x.PublishingId == publishingId && x.Type == ContentTypes.AppConfigV2);

            var originalContentStream = new MemoryStream(zipContent);
            using var originalZipArchive = new ZipArchive(originalContentStream);
            foreach (var i in resignedAppConfigContent)
            {

                Assert.Equal(sourceAppConfigContent1.Created, i.Created);
                Assert.Equal(sourceAppConfigContent1.Release, i.Release);

                var s = new MemoryStream(i.Content);
                using var z = new ZipArchive(s);

                Assert.True(Enumerable.SequenceEqual(originalZipArchive.ReadEntry(ZippedContentEntryNames.Content), z.ReadEntry(ZippedContentEntryNames.Content)));
                Assert.NotEqual(originalZipArchive.GetEntry(ZippedContentEntryNames.NlSignature), z.GetEntry(ZippedContentEntryNames.NlSignature));
            }

            //Repeating should have no effect
            await resigner.ExecuteAsync(ContentTypes.AppConfig, ContentTypes.AppConfigV2, ZippedContentEntryNames.Content);
            Assert.Equal(4, _contentDbContext.Content.Count());
        }
    }
}
