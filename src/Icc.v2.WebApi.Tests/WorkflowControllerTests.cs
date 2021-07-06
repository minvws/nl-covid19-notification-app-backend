// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Icc.V2.WebApi.Tests
{
    public abstract class WorkflowControllerTests : WebApplicationFactory<Startup>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly WorkflowDbContext _workflowDbContext;

        protected WorkflowControllerTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions);
            _workflowDbContext.Database.EnsureCreated();

            _factory = WithWebHostBuilder(
                builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddScoped(sp => new WorkflowDbContext(workflowDbContextOptions));
                    });
                });
        }

        #region Lab ConfirmationId tests
        [Fact]
        public async Task PutPubTek_ReturnsOkAndTrueResult_When_ConfirmationId_IsValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "222222",
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkAndTrueResult_When_ConfirmationId_IsNotValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "111111",
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }
        #endregion


        [Fact]
        public async Task PutPubTek_ReturnsOkAndTrueResult_When_PubTEK_IsValid()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJQ",
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkAndFalseResult_When_PubTEK_HasInValidCharacter()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "18T6LJQ",
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkAndFalseResult_When_PubTEK_HasInValidCheckCode()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJR",
                SelectedDate = DateTime.Today,
                Symptomatic = true,
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.False(result.Valid);
        }

        [Fact]
        public async Task PutPubTek_ReturnsOkAndTrueResult_When_SubjectHasSymptoms_Is_False_And_DateOfTest_HasValue()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJQ",
                SelectedDate = null,
                Symptomatic = false
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }


        [Fact]
        public async Task PutPubTek_ReturnsOkAndTrueResult_When_SubjectHasSymptoms_Is_True_And_DateOfSymptomsOnset_HasValue()
        {
            // Arrange
            var args = new PublishTekArgs
            {
                GGDKey = "L8T6LJQ",
                SelectedDate = DateTime.Today,
                Symptomatic = true
            };

            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            _workflowDbContext.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
            {
                GGDKey = args.GGDKey,
                StartDateOfTekInclusion = args.SelectedDate,

            });
            await _workflowDbContext.SaveChangesAsync();

            var client = _factory.CreateClient();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var responseMessage = await client.PutAsync($"{EndPointNames.CaregiversPortalApi.PubTek}", content, token);

            // Assert
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.True(result.Valid);
        }
    }
}
