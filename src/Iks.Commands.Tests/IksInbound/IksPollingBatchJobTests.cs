// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.IksInbound
{
    public class IksPollingBatchJobTests
    {
        private readonly IDbProvider<IksInDbContext> _IksInDbProvider;

        public IksPollingBatchJobTests()
        {
            _IksInDbProvider = new SqliteInMemoryDbProvider<IksInDbContext>();
        }

        [Fact]
        public async void Tests_that_entire_sequence_of_batches_are_downloaded()
        {
            // Assemble: test state
            var downloadedBatches = new List<IksWriteArgs>();

            // Assemble: other object
            var logger = new Mock<ILogger<IksPollingBatchJob>>();
            var now = DateTime.UtcNow;
            var dtp = new Mock<IUtcDateTimeProvider>();
            dtp.Setup(_ => _.Snapshot).Returns(now);
            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            // Assemble: configure the receiver to return the first sequence of files
            var responses = new List<HttpGetIksSuccessResult>
            {
                new HttpGetIksSuccessResult {BatchTag = "1", Content = new byte[] {0x0, 0x0}, NextBatchTag = "2"},
                new HttpGetIksSuccessResult {BatchTag = "2", Content = new byte[] {0x0, 0x0}, NextBatchTag = "3"},
                new HttpGetIksSuccessResult {BatchTag = "3", Content = new byte[] {0x0, 0x0}, NextBatchTag = null}
            };
            var receiver = new FixedResultHttpGetIksCommand(responses);

            // Assemble: create the job to be tested
            IksPollingBatchJob sut = new IksPollingBatchJob(dtp.Object, () => receiver, () => writer.Object, _IksInDbProvider.CreateNew(), new EfgsConfig(), logger.Object);

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(3, downloadedBatches.Count);
            Assert.Equal("1", downloadedBatches[0].BatchTag);
            Assert.Equal("2", downloadedBatches[1].BatchTag);
            Assert.Equal("3", downloadedBatches[2].BatchTag);
        }
        
        [Fact]
        public async void Tests_that_batch_is_only_downloaded_once()
        {
            // Assemble: test state
            var downloadedBatches = new List<IksWriteArgs>();

            // Assemble: other object
            var logger = new Mock<ILogger<IksPollingBatchJob>>();
            var now = DateTime.UtcNow;
            var dtp = new Mock<IUtcDateTimeProvider>();
            dtp.Setup(_ => _.Snapshot).Returns(now);
            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            // Assemble: configure the receiver to return the first sequence of files
            var responses = new List<HttpGetIksSuccessResult>
            {
                new HttpGetIksSuccessResult {BatchTag = "1", Content = new byte[] {0x0, 0x0}, NextBatchTag = null}
            };
            var receiver = new FixedResultHttpGetIksCommand(responses);

            // Assemble: create the job to be tested
            IksPollingBatchJob sut = new IksPollingBatchJob(dtp.Object, () => receiver, () => writer.Object, _IksInDbProvider.CreateNew(), new EfgsConfig(), logger.Object);

            // Act
            await sut.ExecuteAsync();
            await sut.ExecuteAsync();
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(downloadedBatches);
        }

        [Fact]
        public async void Tests_that_batches_added_between_calls_are_downloaded()
        {
            // Assemble: test state
            var downloadedBatches = new List<IksWriteArgs>();

            // Assemble: other object
            var logger = new Mock<ILogger<IksPollingBatchJob>>();

            var now = DateTime.UtcNow;
            var dtp = new Mock<IUtcDateTimeProvider>();
            dtp.Setup(_ => _.Snapshot).Returns(now);
            var writer = new Mock<IIksWriterCommand>();
            writer.Setup(_ => _.Execute(It.IsAny<IksWriteArgs>()))
                .Callback((IksWriteArgs args) => downloadedBatches.Add(args));

            // Assemble: configure the receiver to return the first sequence of files
            var responses = new List<HttpGetIksSuccessResult>
            {
                new HttpGetIksSuccessResult {BatchTag = "1", Content = new byte[] {0x0, 0x0}, NextBatchTag = null}
            };
            var receiver = new FixedResultHttpGetIksCommand(responses);

            // Assemble: create the job to be tested
            IksPollingBatchJob sut = new IksPollingBatchJob(dtp.Object, () => receiver, () => writer.Object, _IksInDbProvider.CreateNew(), new EfgsConfig(), logger.Object);

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(downloadedBatches);

            // Assemble: add another batch
            receiver.AddItem(new HttpGetIksSuccessResult {BatchTag = "2", Content = new byte[] {0x0, 0x0}, NextBatchTag = null});

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(2, downloadedBatches.Count);
            Assert.Equal("1", downloadedBatches[0].BatchTag);
            Assert.Equal("2", downloadedBatches[1].BatchTag);
        }

        // Day -2: A
        // Day -1: B
        // Day:    C
        public void ThirdTest()
        {
            // Assemble

            // Act

            // Assert
        }
    }

    /// <summary>
    /// IHttpGetIksCommand which returns the provided results in order.
    /// Use for tests.
    /// </summary>
    class FixedResultHttpGetIksCommand : IIHttpGetIksCommand
    {
        private readonly List<HttpGetIksSuccessResult> _Responses;
        private int _CallIndex;

        public FixedResultHttpGetIksCommand(List<HttpGetIksSuccessResult> responses)
        {
            _Responses = responses;
        }
        
        public void AddItem(HttpGetIksSuccessResult item)
        {
            if (_Responses.Count > 0) _Responses.Last().NextBatchTag = item.BatchTag;
            _Responses.Add(item);
        }

        public Task<HttpGetIksSuccessResult?> ExecuteAsync(string batchTag, DateTime date)
        {
            HttpGetIksSuccessResult? result = null;

            if(_CallIndex < _Responses.Count)
            {
                result = _Responses[_CallIndex++];
            }

            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Stub for my config
    /// </summary>
    class EfgsConfig : IEfgsConfig
    {
        public string BaseUrl => "";
        public bool SendClientAuthenticationHeaders => true;
        public int DaysToDownload => 1;
        public int MaxBatchesPerRun => 10;
        public bool UploaderEnabled => true;
        public bool DownloaderEnabled => true;
    }
}
