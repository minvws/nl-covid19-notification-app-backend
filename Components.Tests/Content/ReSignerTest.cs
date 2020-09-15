// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Azure.Amqp.Serialization;
using Microsoft.EntityFrameworkCore;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Resources;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{

    [ExclusivelyUses("ReSignerTest1")]
    public class ReSignerTest
    {
        private static readonly Random _Random = new Random();

        [Fact]
        public void ResignManifest()
        {
            var lf = new LoggerFactory();

            //Add some db rows to Content
            Func<ContentDbContext> dbp = () =>
            {
                var y = new DbContextOptionsBuilder();
                y.UseSqlServer("Data Source=.;Initial Catalog=ReSignerTest1;Integrated Security=True");
                return new ContentDbContext(y.Options);
            };

            var dbc = dbp();
            var db = dbc.Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            var d = DateTime.Now;

            using var testContentStream = ResourcesHook.GetManifestResourceStream("ResignTestManifest.zip");

            using var m = new MemoryStream();
            testContentStream.CopyTo(m);
            var zipContent = m.ToArray();

            var m1 = new ContentEntity { Content = zipContent, PublishingId = "1", ContentTypeName = "Meh", Type = ContentTypes.Manifest, Created = d, Release = d };

            dbc.Content.AddRange(new [] {
                m1,
                new ContentEntity { Content = new byte[0], PublishingId = "2", ContentTypeName = "Meh", Type = ContentTypes.AppConfig, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "3", ContentTypeName = "Meh", Type = ContentTypes.AppConfigV2, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "4", ContentTypeName = "Meh", Type = ContentTypes.ExposureKeySet, Created = d, Release = d },
                new ContentEntity { Content = new byte[0], PublishingId = "5", ContentTypeName = "Meh", Type = ContentTypes.ExposureKeySetV2, Created = d, Release = d },
                });

            dbc.SaveChanges();

            //resign some
            var signer = new CmsSignerEnhanced(
                new EmbeddedResourceCertificateProvider(new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"), lf.CreateLogger<EmbeddedResourceCertificateProvider>()), //Not a secret.
                //TODO add a better test chain.
                new EmbeddedResourcesCertificateChainProvider(new HardCodedCertificateLocationConfig("StaatDerNLChain-Expires2020-08-28.p7b", "")), //Not a secret.
                new StandardUtcDateTimeProvider()
            );

            var resigner = new NlContentResignCommand(dbp, signer);
            resigner.Execute(ContentTypes.Manifest, ContentTypes.ManifestV2, ZippedContentEntryNames.Content).GetAwaiter().GetResult();

            //check the numbers
            Assert.Equal(6, dbc.Content.Count());

            var m2 = dbc.Content.Single(x => x.PublishingId == "1" && x.Type == ContentTypes.ManifestV2);

            Assert.Equal(m1.Created, m2.Created);
            Assert.Equal(m1.Release, m2.Release);

            var ms1 = new MemoryStream(zipContent);
            using var zip1 = new ZipArchive(ms1);

            var ms2 = new MemoryStream(m2.Content);
            using var zip2 = new ZipArchive(ms2);

            Assert.True(Enumerable.SequenceEqual(zip1.ReadEntry(ZippedContentEntryNames.Content), zip2.ReadEntry(ZippedContentEntryNames.Content)));
            Assert.NotEqual(zip1.GetEntry(ZippedContentEntryNames.NLSignature), zip2.GetEntry(ZippedContentEntryNames.NLSignature));
        }
    }
}
