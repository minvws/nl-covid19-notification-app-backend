// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ManifestEngine.Tests
{
    public abstract class ManifestUpdateCommandTest : IDisposable
    {
        private readonly IDbProvider<ContentDbContext> _contentDbProvider;
        private readonly ManifestUpdateCommand _sut;
        private Mock<IUtcDateTimeProvider> _dateTimeProviderMock;
        private DateTime _mockedTime = DateTime.UtcNow;

        public ManifestUpdateCommandTest(IDbProvider<ContentDbContext> contentDbProvider)
        {
            _contentDbProvider = contentDbProvider ?? throw new ArgumentException();
            
            var nlSignerMock = new Mock<IContentSigner>();
            nlSignerMock.Setup(x => x.GetSignature(new byte[0]))
                .Returns(new byte[] { 2 });
            
            var eksConfigMock = new Mock<IEksConfig>(MockBehavior.Strict);
            eksConfigMock.Setup(x => x.LifetimeDays)
                .Returns(14);
 
            _dateTimeProviderMock = new Mock<IUtcDateTimeProvider>();
                _dateTimeProviderMock.Setup(x => x.Snapshot)
                    .Returns(_mockedTime);
            
            var loggerFactory = new LoggerFactory();
            var jsonSerializer = new StandardJsonSerializer();
            var loggingExtensionsMock = new ManifestUpdateCommandLoggingExtensions(
                loggerFactory.CreateLogger<ManifestUpdateCommandLoggingExtensions>());

            Func<IContentEntityFormatter> ContentFormatterInjector = () => 
                new StandardContentEntityFormatter(
                    new ZippedSignedContentFormatter(nlSignerMock.Object),
                    new Sha256HexPublishingIdService(),
                    jsonSerializer);

            _sut = new ManifestUpdateCommand(
                new ManifestV2Builder(_contentDbProvider.CreateNew(), eksConfigMock.Object, _dateTimeProviderMock.Object),
                new ManifestV3Builder(_contentDbProvider.CreateNew(), eksConfigMock.Object, _dateTimeProviderMock.Object),
                new ManifestV4Builder(_contentDbProvider.CreateNew(), eksConfigMock.Object, _dateTimeProviderMock.Object),
                _contentDbProvider.CreateNew,
                loggingExtensionsMock,
                _dateTimeProviderMock.Object,
                jsonSerializer,
                ContentFormatterInjector
            );
        }

        [Fact]
        public async Task NoManifestInDb_ExecuteAll_ThreeManifestsInDb()
        {
            //Act
            await _sut.ExecuteAllAsync();

            //Assert
            Assert.Equal(3, _contentDbProvider.CreateNew().Content.Count());
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV3)); 
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV4)); 
        }

        [Fact]
        public async Task NoManifestInDb_ExecuteAllTwice_ThreeManifestsInDb()
        {
            //Act
            await _sut.ExecuteAllAsync();
            
            //Advance the clock
            _dateTimeProviderMock.Setup(x => x.Snapshot)
                .Returns(_mockedTime.AddMinutes(2));
            
            await _sut.ExecuteAllAsync();

            //Assert
            Assert.Equal(3, _contentDbProvider.CreateNew().Content.Count());
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV2));
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV3));
            Assert.Equal(1, _contentDbProvider.CreateNew().Content.Count(x => x.Type == ContentTypes.ManifestV4));
        }

        public void Dispose()
        {
            _contentDbProvider.Dispose();
        }
    }
}