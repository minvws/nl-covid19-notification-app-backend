using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.Api.IccBackend;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace Api.IccBackend.Tests
{
    public class WorkflowControllerTests
    {
        private readonly CustomWebApplicationFactory<Startup, WorkflowDbContext> _Factory;

        public WorkflowControllerTests()
        {
            _Factory = new CustomWebApplicationFactory<Startup, WorkflowDbContext>();
        }
        
        [Fact]
        public async Task PostAuthorise_ReturnsOkResult_When_ConfirmationId_IsValid()
        {
            // Arrange
            var args = new AuthorisationArgs
            {
                LabConfirmationId = "222222",
                DateOfSymptomsOnset = DateTime.Today
            };

            var client = _Factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var sp = services.BuildServiceProvider();

                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<WorkflowDbContext>();

                            db.Database.EnsureCreated();

                            db.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
                            {
                                LabConfirmationId = args.LabConfirmationId,
                                DateOfSymptomsOnset = args.DateOfSymptomsOnset
                            });
                            db.SaveChanges();
                        }
                    });
                })
                .CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;
            
            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = {ContentType = new MediaTypeHeaderValue("application/json")}
            };

            // Act
            var result = await client.PostAsync($"{EndPointNames.CaregiversPortalApi.LabConfirmation}", content, token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task PostAuthorise_ReturnsBadRequestResult_When_ConfirmationId_IsNotValid()
        {
            // Arrange
            var args = new AuthorisationArgs
            {
                LabConfirmationId = "111111",
                DateOfSymptomsOnset = DateTime.Today
            };

            var client = _Factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var sp = services.BuildServiceProvider();

                        using (var scope = sp.CreateScope())
                        {
                            var scopedServices = scope.ServiceProvider;
                            var db = scopedServices.GetRequiredService<WorkflowDbContext>();

                            db.Database.EnsureCreated();

                            db.KeyReleaseWorkflowStates.Add(new TekReleaseWorkflowStateEntity
                            {
                                LabConfirmationId = args.LabConfirmationId,
                                DateOfSymptomsOnset = args.DateOfSymptomsOnset
                            });
                            db.SaveChanges();
                        }
                    });
                })
                .CreateClient();


            var source = new CancellationTokenSource();
            var token = source.Token;

            var content = new StringContent(JsonSerializer.Serialize(args))
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };

            // Act
            var result = await client.PostAsync($"{EndPointNames.CaregiversPortalApi.LabConfirmation}", content, token);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
