// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ManifestUpdateCommand;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EmbeddedCertProvider;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using Xunit;
using System.IO;
using System.IO.Compression;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
	public class ManifestV3CreationTest
	{
		private Func<ContentDbContext> _ContentDbFactory;

		public ManifestV3CreationTest()
		{
			_ContentDbFactory = () =>
				new ContentDbContext(
					new DbContextOptionsBuilder()
						.UseSqlServer("Initial Catalog=ContentTest1; Server=.;Persist Security Info=True;Integrated Security=True;Connection Timeout=60;")
						.Options);

			var contentDb = _ContentDbFactory();
			contentDb.Database.EnsureDeleted();
			contentDb.Database.EnsureCreated();
		}
		
		[Fact]
		public void ManifestUpdateCommand_ExecuteForV3()
		{
			//Arrange
			PopulateContentDb();

			var sut = CompileManifestUpdateCommand();

			//Act
			sut.ExecuteForV3().GetAwaiter().GetResult();

			var database = _ContentDbFactory();
			var result = database.SafeGetLatestContent(ContentTypes.ManifestV3, DateTime.Now).GetAwaiter().GetResult();

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
			var certProviderLogger = new EmbeddedCertProviderLoggingExtensions(lf.CreateLogger<EmbeddedCertProviderLoggingExtensions>());
			
			var signer = new CmsSignerEnhanced(
				new EmbeddedResourceCertificateProvider(
					new HardCodedCertificateLocationConfig("TestRSA.p12", "Covid-19!"), //Not a secret.
					certProviderLogger),
				new EmbeddedResourcesCertificateChainProvider(
					new HardCodedCertificateLocationConfig("StaatDerNLChain-Expires2020-08-28.p7b", "")), //Not a secret.
				dateTimeProvider);

			Func<IContentEntityFormatter> formatterForV3 = () =>
				new StandardContentEntityFormatter(
					new ZippedSignedContentFormatter(
						signer),
					new Sha256HexPublishingIdService(),
					jsonSerialiser
						);

			var result = new ManifestUpdateCommand(
				new ManifestBuilder(
					_ContentDbFactory(),
					eksConfigMock.Object,
					dateTimeProvider),
				new ManifestBuilderV3(
					_ContentDbFactory(), 
					eksConfigMock.Object,
					dateTimeProvider),
				_ContentDbFactory,
				new ManifestUpdateCommandLoggingExtensions(lf.CreateLogger<ManifestUpdateCommandLoggingExtensions>()),
				dateTimeProvider,
				jsonSerialiser,
				entityFormatterMock.Object,
				formatterForV3
				);

			return result;
		}

		private void PopulateContentDb()
		{
			var database = _ContentDbFactory();
			var yesterday = DateTime.Now.AddDays(-1);
			string content = "This is a ResourceBundleV3";

			database.Content.AddRange(new[]
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

			database.SaveChanges();
		}

	}
}
