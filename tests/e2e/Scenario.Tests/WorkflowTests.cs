using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Core.E2ETests;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using Xunit;

namespace Scenario.Tests
{
    [Trait("test", "e2e")]
    public class WorkflowTests : TestBase
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public WorkflowTests()
        {
            _randomNumberGenerator = new StandardRandomNumberGenerator();
        }

        [Theory]
        [InlineData("test")]
        [InlineData("acc")]
        [InlineData("prod")]
        public async Task Get_MobileApi_Endpoint_Returns_Http200(string environment)
        {
            // Arrange
            var appClient = new AppClient();

            // Act
            var responseMessage = await appClient.GetAsync(new Uri($"{Config.AppBaseUrl(environment)}"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode); // I should have received the manifest
        }

        [Theory]
        [InlineData("test")]
        public async Task Calling_RegisterEndpoint_Result_In_New_Workflow_Entry(string environment)
        {
            var client = new AppClient();

            // Register
            // Act
            var (responseMessage, enrollmentResponse) = await client.PostAsync<EnrollmentResponseV2>(new Uri($"{Config.AppBaseUrl(environment)}"), $"v2", $"{Config.RegisterEndPoint}", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.NotNull(responseMessage.Headers.ToString().Split("padding: ")[1]); // Padding should be added in the header
            Assert.NotNull(enrollmentResponse.BucketId);
            Assert.NotNull(enrollmentResponse.ConfirmationKey);
            Assert.NotNull(enrollmentResponse.GGDKey);

            // Post Keys
            // Arrange
            var args = GenerateTeks(enrollmentResponse.BucketId);

            var tekDates = args.Keys
                .OrderBy(x => x.RollingStartNumber)
                .Select(x => new { x, Date = x.RollingStartNumber.FromRollingStartNumber() });

            foreach (var i in tekDates)
            {
                Trace.WriteLine($"RSN:{i.x.RollingStartNumber} Date:{i.Date:yyyy-MM-dd}.");
            }

            var data = args.ToByteArray();

            var signature = HttpUtility.UrlEncode(Sign(Convert.FromBase64String(enrollmentResponse.ConfirmationKey), data));
            var postkeysContent = new ByteArrayContent(data);
            postkeysContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var postkeysResult = await client.PostAsync(new Uri($"{Config.AppBaseUrl(environment)}"), "v1", $"postkeys?sig={signature}", postkeysContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, postkeysResult.StatusCode);

            // Authorize
            // Arrange
            var publishTekArgs = new PublishTekArgs
            {
                GGDKey = enrollmentResponse.GGDKey, SubjectHasSymptoms = true, DateOfSymptomsOnset = DateTime.Today
            };

            var pubtekContent = new StringContent(publishTekArgs.ToJson(), Encoding.UTF8, "application/json");

            // Act
            var pubTekResult = await client.PutAsync(new Uri($"{Config.IccApiBaseUrl(environment)}"), "pubtek", pubtekContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, pubTekResult.StatusCode);

        }

        private PostTeksArgs GenerateTeks(string bucketId)
        {
            var keys = new List<PostTeksItemArgs>();

            for (var i = 0; i < 10; i++)
            {
                keys.Add(new PostTeksItemArgs
                {
                    RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber(),
                    RollingPeriod = _randomNumberGenerator.Next(1, UniversalConstants.RollingPeriodRange.Hi),
                    KeyData = Convert.ToBase64String(_randomNumberGenerator.NextByteArray(UniversalConstants.DailyKeyDataByteCount))
                });
            }

            return new PostTeksArgs { BucketId = bucketId, Keys = keys.ToArray(), Padding = "ZGVmYXVsdA==" };
        }

        private static string Sign(in byte[] key, in byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(data);
            var hashBase64 = Convert.ToBase64String(hash);

            return hashBase64;
        }
    }
}
