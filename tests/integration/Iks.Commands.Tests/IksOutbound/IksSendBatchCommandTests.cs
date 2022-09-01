// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksOutbound
{
    public abstract class IksSendBatchCommandTests
    {
        private readonly IksOutDbContext _iksOutDbContext;

        private readonly EfgsConfigMock _efgsConfigFake = new();
        private readonly Mock<IAuthenticationCertificateProvider> _certProviderMock;
        private readonly Mock<IFileSystemCertificateConfig> _certificateConfigMock;
        private readonly Mock<IBatchTagProvider> _batchTagProviderMock;
        private readonly Mock<IHsmSignerService> _hsmSignerServiceMock;

        public IksSendBatchCommandTests(DbContextOptions<IksOutDbContext> iksOutDbContextOptions)
        {
            _iksOutDbContext = new IksOutDbContext(iksOutDbContextOptions ?? throw new ArgumentNullException(nameof(iksOutDbContextOptions)));
            _iksOutDbContext.Database.EnsureCreated();
            _certProviderMock = new Mock<IAuthenticationCertificateProvider>();
            _certificateConfigMock = new Mock<IFileSystemCertificateConfig>();
            _batchTagProviderMock = new Mock<IBatchTagProvider>();

            _certificateConfigMock.Setup(x => x.CertificatePath).Returns(It.IsAny<string>());
            _certificateConfigMock.Setup(x => x.CertificateFileName).Returns(It.IsAny<string>());

            var mockCertificate = new Mock<X509Certificate2>();
            _certProviderMock.Setup(p => p.GetCertificate()).Returns(mockCertificate.Object);

            //TODO: remove or replace with Efgs signing settings from HsmConfig
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string> {
                        {"Certificates:EfgsAuthentication:Thumbprint", "Thumbprint"},
                        {"Certificates:EfgsAuthentication:RootTrusted", "false"},
                    }
                )
                .Build();
        }

        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Theory]
        public async Task IksSendBatchCommand_Execute_Returns_Result_In_Sent(HttpStatusCode httpStatusCode)
        {
            // Arrange
            await IksOutDb();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent("")
            };

            var sut = CreateSut(responseMessage);

            // Act
            var result = (IksSendBatchResult)await sut.ExecuteAsync();

            // Assert
            Assert.True(result.Success);
            Assert.False(result.HasErrors);
            Assert.Equal(httpStatusCode, result.Sent.First().StatusCode);

            var entity = _iksOutDbContext.Iks.FirstOrDefault();
            Assert.False(entity.CanRetry);
            Assert.False(entity.Error);
            Assert.Equal(ProcessState.Sent.ToString(), entity.ProcessState);
        }

        [InlineData(HttpStatusCode.MultiStatus)]
        [InlineData(HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(HttpStatusCode.Forbidden)]
        [Theory]
        public async Task IksSendBatchCommand_Execute_Result_In_Invalid(HttpStatusCode httpStatusCode)
        {
            // Arrange
            await IksOutDb();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent("")
            };

            var sut = CreateSut(responseMessage);

            // Act
            var result = (IksSendBatchResult)await sut.ExecuteAsync();

            // Assert
            Assert.False(result.Success);
            Assert.False(result.HasErrors);
            Assert.Equal(httpStatusCode, result.Sent.First().StatusCode);

            var entity = _iksOutDbContext.Iks.FirstOrDefault();
            Assert.False(entity.CanRetry);
            Assert.True(entity.Error);
            Assert.Equal(ProcessState.Invalid.ToString(), entity.ProcessState);
        }

        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(null)]
        [Theory]
        public async Task IksSendBatchCommand_Execute_Result_In_Failed(HttpStatusCode httpStatusCode)
        {
            // Arrange
            await IksOutDb();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent("")
            };

            var sut = CreateSut(responseMessage);

            // Act
            var result = (IksSendBatchResult)await sut.ExecuteAsync();

            // Assert
            Assert.False(result.Success);
            Assert.False(result.HasErrors);
            Assert.Equal(httpStatusCode, result.Sent.First().StatusCode);

            var entity = _iksOutDbContext.Iks.FirstOrDefault();

            switch (httpStatusCode)
            {
                case HttpStatusCode.BadRequest:
                    Assert.False(entity.CanRetry);
                    break;
                case HttpStatusCode.InternalServerError:
                    Assert.True(entity.CanRetry);
                    break;
            }

            Assert.True(entity.Error);
            Assert.Equal(ProcessState.Failed.ToString(), entity.ProcessState);
        }

        [InlineData(HttpStatusCode.NotAcceptable)]
        [Theory]
        public async Task IksSendBatchCommand_Execute_Result_In_Skipped(HttpStatusCode httpStatusCode)
        {
            // Arrange
            await IksOutDb();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent("")
            };

            var sut = CreateSut(responseMessage);

            // Act
            var result = (IksSendBatchResult)await sut.ExecuteAsync();

            // Assert
            Assert.False(result.Success);
            Assert.False(result.HasErrors);
            Assert.Equal(httpStatusCode, result.Sent.First().StatusCode);

            var entity = _iksOutDbContext.Iks.FirstOrDefault();
            Assert.False(entity.CanRetry);
            Assert.True(entity.Error);
            Assert.Equal(ProcessState.Skipped.ToString(), entity.ProcessState);
        }

        private async Task IksOutDb()
        {
            await _iksOutDbContext.BulkDeleteAsync(_iksOutDbContext.Iks.ToList());

            var iksFormatter = new IksFormatter();
            var args = new InteropKeyFormatterArgs[]
            {
               new InteropKeyFormatterArgs {
                   DaysSinceSymtpomsOnset = 0,
                   Origin = "Local",
                   ReportType = (global::Iks.Protobuf.EfgsReportType)EfgsReportType.Unknown,
                   TransmissionRiskLevel = 0,
                   Value = new DailyKey(new byte[]{1}, 1, DateTime.UtcNow.Date.ToRollingStartNumber())
               }
            };

            var bytes = iksFormatter.Format(args);

            _iksOutDbContext.Iks.Add(new Uploader.Entities.IksOutEntity
            {
                Created = DateTime.Today,
                ValidFor = DateTime.Today.AddDays(1),
                Content = bytes,
                Sent = false,
                Qualifier = 1,
                Error = false,
                ProcessState = ProcessState.New.ToString(),
                RetryCount = 0
            });

            await _iksOutDbContext.SaveChangesAsync();
        }

        private IksSendBatchCommand CreateSut(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(responseMessage);

            return new IksSendBatchCommand(
                new IksUploadService(
                    new HttpClient(handlerMock.Object),
                    _efgsConfigFake,
                    _certProviderMock.Object,
                    _certificateConfigMock.Object,
                    new NullLogger<IksUploadService>()),
                _iksOutDbContext,
                _batchTagProviderMock.Object,
                _hsmSignerServiceMock.Object,
                new NullLogger<IksSendBatchCommand>()
                );
        }
    }
}
