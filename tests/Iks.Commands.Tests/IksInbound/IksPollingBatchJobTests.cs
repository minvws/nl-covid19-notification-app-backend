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
        private static DbConnection connection;
        private Mock<IUtcDateTimeProvider> _dtp;
        private DateTime _now;
        private string _dateString;
        private IksDownloaderLoggingExtensions _logger;
        private readonly EfgsConfigMock _config = new EfgsConfigMock();
        private byte[] _dummyContent = new byte[] { 0x0, 0x0 };
        private Mock<IAuthenticationCertificateProvider> _certProvider;

        public IksPollingBatchJobTests()
        {
            _iksInDbContext = new IksInDbContext(new DbContextOptionsBuilder<IksInDbContext>().UseSqlite(CreateInMemoryDatabase()).Options);
            _iksInDbContext.Database.EnsureCreated();

            _now = DateTime.UtcNow;
            _dateString = _now.Date.ToString("yyyyMMdd");

            _dtp = new Mock<IUtcDateTimeProvider>();
            _dtp.Setup(x => x.Snapshot).Returns(_now);

            _logger = new IksDownloaderLoggingExtensions(new NullLogger<IksDownloaderLoggingExtensions>());
            _certProvider = new Mock<IAuthenticationCertificateProvider>();

            var mockCertificate = new Mock<X509Certificate2>();
            _certProvider.Setup<X509Certificate2>(p => p.GetCertificate()).Returns(mockCertificate.Object);
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
                new HttpGetIksResult { BatchTag = firstBatchTag, Content = _dummyContent, NextBatchTag = secondBatchTag, RequestedDay = yesterday },
                new HttpGetIksResult { BatchTag = secondBatchTag, Content = _dummyContent, NextBatchTag = thirdBatchTag, RequestedDay = yesterday },
                new HttpGetIksResult { BatchTag = thirdBatchTag, Content = _dummyContent, NextBatchTag = null, RequestedDay = yesterday }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses, yesterday);

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
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
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config, _logger);

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
            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var responses = new List<HttpGetIksResult>
            {
                new HttpGetIksResult { BatchTag = $"{_dateString}-1", Content = _dummyContent, NextBatchTag = null }
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
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
                new HttpGetIksResult {BatchTag = firstBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = secondBatchTag, RequestedDay = _now.Date},
                new HttpGetIksResult {BatchTag = secondBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = thirdBatchTag, RequestedDay = _now.Date},
                new HttpGetIksResult {BatchTag = thirdBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = firstBatchTag, RequestedDay = _now.Date},
                new HttpGetIksResult {BatchTag = firstBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = thirdBatchTag, RequestedDay = _now.Date},
                new HttpGetIksResult {BatchTag = secondBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = secondBatchTag, RequestedDay = _now.Date},
                new HttpGetIksResult {BatchTag = thirdBatchTag, Content = new byte[] {0x0, 0x0}, NextBatchTag = null, RequestedDay = _now.Date}
            };
            var receiver = FixedResultHttpGetIksCommand.Create(responses);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
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
            var dateStringTomorrow = _dtp.Object.Snapshot.Date.AddDays(1).ToString("yyyyMMdd");

            var responses = new List<HttpGetIksResult>()
            {
                new HttpGetIksResult {BatchTag = $"{_dateString}-1", Content = _dummyContent, NextBatchTag = $"{_dateString}-2", RequestedDay = yesterday},
                new HttpGetIksResult {BatchTag = $"{_dateString}-2", Content = _dummyContent, NextBatchTag = null, RequestedDay = yesterday},
            };

            var receiver = FixedResultHttpGetIksCommand.Create(responses, yesterday);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
                _logger);

            // Act
            sut.Run();
            var resultFirstRun = downloadedBatches.Count;

            // Add batches for second day to receiver and rerun downloader
            receiver.AddItem(
                new HttpGetIksResult { BatchTag = $"{dateStringTomorrow}-1", Content = _dummyContent, NextBatchTag = $"{dateStringTomorrow}-2", RequestedDay = _now },
                _now);

            receiver.AddItem(
                new HttpGetIksResult { BatchTag = $"{dateStringTomorrow}-2", Content = _dummyContent, NextBatchTag = null, RequestedDay = _now },
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
        public async void EfgsReturns400_ExecuteAsync_NextDayIsRequested()
        {
            //Arrange
            var firstUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{_now.AddDays(-2):yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var secondUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{_now.AddDays(-1):yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var thirdUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{_now:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);

            var firstResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var secondResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_dummyContent)
            };
            secondResponseMessage.Headers.Add("batchTag", "today");

            var thirdResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(_dummyContent),
            };
            thirdResponseMessage.Headers.Add("batchTag", "tomorrow");

            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == firstUri), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(firstResponseMessage);
            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == secondUri), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(secondResponseMessage);
            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == thirdUri), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(thirdResponseMessage);

            var writer = new Mock<IIksWriterCommand>();
            var downloadedBatches = new List<IksWriteArgs>();

            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var receiver = new HttpGetIksCommand(
                _config,
                _certProvider.Object,
                _logger,
                mockHttpClientHandler.Object);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
                _logger);

            //Act
            sut.Run();

            //Assert
            Assert.Equal(2, downloadedBatches.Count);
            Assert.Equal("today", downloadedBatches[0].BatchTag);
            Assert.Equal("tomorrow", downloadedBatches[1].BatchTag);
        }

        [Fact]
        public void EFGSReturns404_ExecuteAsync_NextDayIsRequested()
        {
            //Arrange
            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            var writer = new Mock<IIksWriterCommand>();

            var yesterday = _now.AddDays(-1);
            var downloadedBatches = new List<IksWriteArgs>();

            var testUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{yesterday:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);

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

            writer.Setup(x => x.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var receiver = new HttpGetIksCommand(
                _config,
                _certProvider.Object,
                _logger,
                mockHttpClientHandler.Object);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
                _logger);

            //Act
            sut.Run();

            //Assert
            var jobResultState = _iksInDbContext.InJob.SingleOrDefaultAsync().GetAwaiter().GetResult();

            Assert.Empty(downloadedBatches);
            Assert.Equal($"{yesterday:yyyy-MM-dd}", jobResultState.LastBatchTag);
        }

        [Fact]
        public void EFGSReturns410_ExecuteAsync_NextBatchIsRequested()
        {
            //Arrange
            var mockHttpClientHandler = new Mock<HttpClientHandler>();
            var writer = new Mock<IIksWriterCommand>();

            var yesterday = _now.AddDays(-1);
            var downloadedBatches = new List<IksWriteArgs>();

            var testUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{yesterday:yyyy-MM-dd}", UriKind.RelativeOrAbsolute);
            var nextBatchUri = new Uri($"{_config.BaseUrl}/diagnosiskeys/download/{yesterday:yyyy-MM-dd}-1", UriKind.RelativeOrAbsolute);

            var firstResponseMessage = new HttpResponseMessage
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
                .ReturnsAsync(firstResponseMessage);

            mockHttpClientHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri == nextBatchUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(secondResponseMessage);


            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            var receiver = new HttpGetIksCommand(
                _config,
                _certProvider.Object,
                _logger,
                mockHttpClientHandler.Object);

            var sut = new IksPollingBatchJob(
                _dtp.Object,
                receiver,
                writer.Object,
                _iksInDbContext,
                _config,
                _logger);

            //Act
            sut.Run();

            //Assert
            var jobResultState = _iksInDbContext.InJob.SingleOrDefaultAsync().GetAwaiter().GetResult();

            Assert.Single(downloadedBatches);
            Assert.Equal($"{yesterday:yyyy-MM-dd}-1", jobResultState.LastBatchTag);
        }

    }
}
