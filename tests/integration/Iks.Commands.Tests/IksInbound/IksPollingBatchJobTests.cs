// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsDownloader.Jobs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound
{
    public class IksPollingBatchJobTests : IDisposable
    {
        private readonly IksInDbContext _iksInDbContext;
        private readonly IksDownloaderLoggingExtensions _logger;
        private readonly DateTime _now;
        private readonly string _dateString;
        private readonly byte[] _dummyContent = new byte[] { 0x0, 0x0 };

        private readonly Mock<IUtcDateTimeProvider> _dtpMock;
        private readonly EfgsConfigMock _efgsConfigMock = new EfgsConfigMock();
        private readonly Mock<IAuthenticationCertificateProvider> _certProviderMock;
        private readonly Mock<IThumbprintConfig> _thumbprintConfigMock;

        private static DbConnection connection;

        public IksPollingBatchJobTests()
        {
            _iksInDbContext = new IksInDbContext(new DbContextOptionsBuilder<IksInDbContext>().UseSqlite(CreateInMemoryDatabase()).Options);
            _iksInDbContext.Database.EnsureCreated();

            _now = DateTime.UtcNow;
            _dateString = _now.Date.ToString("yyyyMMdd");

            _dtpMock = new Mock<IUtcDateTimeProvider>();
            _dtpMock.Setup(x => x.Snapshot).Returns(_now);

            _logger = new IksDownloaderLoggingExtensions(new NullLogger<IksDownloaderLoggingExtensions>());
            _certProviderMock = new Mock<IAuthenticationCertificateProvider>();
            _thumbprintConfigMock = new Mock<IThumbprintConfig>();

            var mockCertificate = new Mock<X509Certificate2>();
            _certProviderMock.Setup(p => p.GetCertificate(It.IsAny<string>(), It.IsAny<bool>())).Returns(mockCertificate.Object);
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        public void Dispose() => connection.Dispose();

        [Fact]
        public async void ThreeBatchesPresent_ExecuteAsync_AllBatchesDownloaded()
        {
            // Arrange
            var downloadedBatches = new List<IksWriteArgs>();
            var writer = new Mock<IIksWriterCommand>();

            var yesterday = _now.AddDays(-1);
            var firstBatchTag = $"{_dateString}-1";
            var secondBatchTag = $"{_dateString}-2";
            var thirdBatchTag = $"{_dateString}-3";

            var responses = new List<HttpGetIksResult>
            {
                new HttpGetIksResult { BatchTag = firstBatchTag, Content = _dummyContent, NextBatchTag = secondBatchTag },
                new HttpGetIksResult { BatchTag = secondBatchTag, Content = _dummyContent, NextBatchTag = thirdBatchTag },
                new HttpGetIksResult { BatchTag = thirdBatchTag, Content = _dummyContent, NextBatchTag = null }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses, yesterday);

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            // Act
            sut.Run();

            // Assert
            Assert.Equal(3, downloadedBatches.Count);
            Assert.Equal(firstBatchTag, downloadedBatches[0].BatchTag);
            Assert.Equal(secondBatchTag, downloadedBatches[1].BatchTag);
            Assert.Equal(thirdBatchTag, downloadedBatches[2].BatchTag);
        }

        [Fact]
        public async void SingleBatchPresent_ExecuteAsyncThrice_BatchDownloadedOnce()
        {
            // Arrange
            var downloadedBatches = new List<IksWriteArgs>();

            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var responses = new List<HttpGetIksResult>
            {
                new HttpGetIksResult { BatchTag = $"{_dateString}-1", Content = _dummyContent, NextBatchTag = null }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock, _logger);

            // Act
            sut.Run();
            sut.Run();
            sut.Run();

            // Assert
            Assert.Single(downloadedBatches);
        }

        [Fact]
        public async void SecondBatchAddedAfterFirstDownload_ExecuteAsync_BothDownloaded()
        {
            // Arrange
            var downloadedBatches = new List<IksWriteArgs>();

            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var responses = new List<HttpGetIksResult>
            {
                new HttpGetIksResult { BatchTag = $"{_dateString}-1", Content = _dummyContent, NextBatchTag = null }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            // Act
            sut.Run();
            var firstResult = downloadedBatches.Count;

            receiver.AddItem(
                new HttpGetIksResult { BatchTag = $"{_dateString}-2", Content = _dummyContent, NextBatchTag = null });

            sut.Run();

            // Assert
            Assert.Equal(1, firstResult);
            Assert.Equal(2, downloadedBatches.Count);
            Assert.Equal($"{_dateString}-1", downloadedBatches[0].BatchTag);
            Assert.Equal($"{_dateString}-2", downloadedBatches[1].BatchTag);
        }

        [Fact]
        public async void DuplicateBatchesPresent_ExecuteAsync_DuplicatesNotDownloaded()
        {
            // Arrange
            var downloadedBatches = new List<IksWriteArgs>();

            var writer = new Mock<IIksWriterCommand>();

            //might not be required
            Action<IksWriteArgs> persistBatchInDb = (IksWriteArgs batchToPersist) =>
            {
                downloadedBatches.Add(batchToPersist);

                _iksInDbContext.Received.Add(new IksInEntity
                {
                    BatchTag = batchToPersist.BatchTag,
                    Content = batchToPersist.Content,
                    Created = _now
                });

                _iksInDbContext.SaveChanges();
            };

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback(persistBatchInDb);

            var firstBatchTag = $"{_dateString}-1";
            var secondBatchTag = $"{_dateString}-2";
            var thirdBatchTag = $"{_dateString}-3";

            var responses = new List<HttpGetIksResult>
            {
                new HttpGetIksResult {BatchTag = firstBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = secondBatchTag },
                new HttpGetIksResult {BatchTag = secondBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = thirdBatchTag },
                new HttpGetIksResult {BatchTag = thirdBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = firstBatchTag },
                new HttpGetIksResult {BatchTag = firstBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = thirdBatchTag },
                new HttpGetIksResult {BatchTag = secondBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = secondBatchTag },
                new HttpGetIksResult {BatchTag = thirdBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = null }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            // Act
            sut.Run();

            // Assert
            Assert.Equal(3, downloadedBatches.Count);
            Assert.Equal(firstBatchTag, downloadedBatches[0].BatchTag);
            Assert.Equal(secondBatchTag, downloadedBatches[1].BatchTag);
            Assert.Equal(thirdBatchTag, downloadedBatches[2].BatchTag);
        }

        [Fact]
        public async void NewBatchesForNextDayAddedAfterFirstDownload_ExecuteAsync_AllDownloadedInOrder()
        {
            // Arrange
            var downloadedBatches = new List<IksWriteArgs>();

            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var yesterday = _now.AddDays(-1);
            var dateStringTomorrow = _dtpMock.Object.Snapshot.Date.AddDays(1).ToString("yyyyMMdd");

            var responses = new List<HttpGetIksResult>()
            {
                new HttpGetIksResult {BatchTag = $"{_dateString}-1", Content = _dummyContent, NextBatchTag = $"{_dateString}-2" },
                new HttpGetIksResult {BatchTag = $"{_dateString}-2", Content = _dummyContent, NextBatchTag = null },
            };

            var receiver = FixedResultHttpGetIksCommand.Create(responses, yesterday);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            // Act
            sut.Run();
            var resultFirstRun = downloadedBatches.Count;

            // Add batches for second day to receiver and rerun downloader
            receiver.AddItem(
                new HttpGetIksResult { BatchTag = $"{dateStringTomorrow}-1", Content = _dummyContent, NextBatchTag = $"{dateStringTomorrow}-2" },
                _now);

            receiver.AddItem(
                new HttpGetIksResult { BatchTag = $"{dateStringTomorrow}-2", Content = _dummyContent, NextBatchTag = null },
                _now);

            sut.Run();

            // Assert
            Assert.Equal(2, resultFirstRun);
            Assert.Equal(4, downloadedBatches.Count);
            Assert.Equal($"{_dateString}-1", downloadedBatches[0].BatchTag);
            Assert.Equal($"{_dateString}-2", downloadedBatches[1].BatchTag);
            Assert.Equal($"{dateStringTomorrow}-1", downloadedBatches[2].BatchTag);
            Assert.Equal($"{dateStringTomorrow}-2", downloadedBatches[3].BatchTag);
        }

        [Fact]
        public async void EfgsReturns400_ExecuteAsync_ExceptionThrownAndJobStateNotStored()
        {
            //Arrange
            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            var downloadedBatches = new List<IksWriteArgs>();

            var testUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{_now.AddDays(-1):yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var secondTestUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{_now:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);

            var testResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var secondResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_dummyContent)
            };
            secondResponseMessage.Headers.Add("batchTag", "today");

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == testUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(testResponseMessage);

            mockHttpClientHandler.Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == secondTestUri),
                   ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(secondResponseMessage);

            var writer = new Mock<IIksWriterCommand>();

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(mockHttpClientHandler.Object);
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var receiver = new HttpGetIksCommand(
                _efgsConfigMock,
                _certProviderMock.Object,
                _thumbprintConfigMock.Object,
                mockHttpClientFactory.Object,
                _logger);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            //Act
            Action releaseThe400 = () => sut.Run();
            var jobResultState = _iksInDbContext.InJob.SingleOrDefaultAsync().GetAwaiter().GetResult();

            //Assert
            Assert.Throws<EfgsCommunicationException>(releaseThe400);
            Assert.Empty(downloadedBatches);
            Assert.Null(jobResultState);
        }

        [Fact]
        public void EFGSReturns404_ExecuteAsync_DownloadHaltedAndJobStateNotStored()
        {
            //Arrange
            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            var writer = new Mock<IIksWriterCommand>();

            var yesterday = _now.AddDays(-1);
            var downloadedBatches = new List<IksWriteArgs>();

            var testUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{yesterday:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var secondTestUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{_now:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            var secondResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_dummyContent)
            };
            secondResponseMessage.Headers.Add("batchTag", "today");

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == testUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == secondTestUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(secondResponseMessage);

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(mockHttpClientHandler.Object);
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var receiver = new HttpGetIksCommand(
                _efgsConfigMock,
                _certProviderMock.Object,
                _thumbprintConfigMock.Object,
                mockHttpClientFactory.Object,
                _logger);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            //Act
            sut.Run();

            //Assert
            var jobResultState = _iksInDbContext.InJob.SingleOrDefaultAsync().GetAwaiter().GetResult();

            Assert.Empty(downloadedBatches);
            Assert.Null(jobResultState);
        }

        [Fact]
        public void EFGSReturns410_ExecuteAsync_NextDayIsRequested()
        {
            //Arrange
            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            var writer = new Mock<IIksWriterCommand>();

            var yesterday = _now.AddDays(-1);
            var downloadedBatches = new List<IksWriteArgs>();

            var testUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{yesterday:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var secondTestUri = new Uri($"{_efgsConfigMock.BaseUrl}/diagnosiskeys/download/{_now:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);

            var firstResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_dummyContent)
            };
            firstResponseMessage.Headers.Add("batchTag", "yesterday");

            var secondResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Gone
            };

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == testUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(firstResponseMessage);

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == secondTestUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(secondResponseMessage);

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(mockHttpClientHandler.Object);
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var receiver = new HttpGetIksCommand(
                _efgsConfigMock,
                _certProviderMock.Object,
                _thumbprintConfigMock.Object,
                mockHttpClientFactory.Object,
                _logger);

            var sut = new IksPollingBatchJob(
                _dtpMock.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _efgsConfigMock,
                _logger);

            //Act
            sut.Run();

            //Assert
            Assert.Single(downloadedBatches);
            Assert.Equal("yesterday", downloadedBatches[0].BatchTag);
        }

    }
}
