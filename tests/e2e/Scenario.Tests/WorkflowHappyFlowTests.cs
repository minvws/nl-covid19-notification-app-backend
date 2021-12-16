using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Core.E2ETests;
using Endpoint.Tests;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using Xunit;

namespace Scenario.Tests
{

    /// <summary>
    /// The tests need to run in a specific order
    /// </summary>
    public enum TestOrder
    {
        CheckEndpoints = 1, // First check if the endpoints are up and running
        Register = 2, // Register a new workflow
        PostKeys = 3, // Post keys to the workflow
        AuthorizePubTek = 4, // Authorize the workflow
        VerifyExposureKeySet = 5 // Get the ExposureKeySet from the CDN

    }

    /// <summary>
    /// Testing the happy flow
    /// A prerequisite is that the already exist a manifest to be fetched from the CDN. Run the ManifestEngine to create one.
    /// </summary>
    [TestCaseOrderer("Core.E2ETests.PriorityOrderer", "Core.E2ETests")]
    [Trait("test", "e2e")]
    public class WorkflowHappyFlowTests : TestBase
    {
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        public static EnrollmentResponse EnrollmentResponseV2;
        public static PostTeksArgs PostTeks;
        public static ManifestContent CurrentManifest;

        public WorkflowHappyFlowTests()
        {
            _randomNumberGenerator = new StandardRandomNumberGenerator();
        }

        [TestPriority((int)TestOrder.CheckEndpoints)]
        [Theory]
        [InlineData("test")]
        //[InlineData("acc")]
        public async Task Get_MobileApi_Endpoint_Returns_Http200(string environment)
        {
            // Arrange
            var appClient = new AppClient();

            // Act
            var responseMessage = await appClient.GetAsync(new Uri($"{Config.AppBaseUrl(environment, true)}"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode); // I should have received the manifest
        }

        /// <summary>
        /// The first step in this e2e test is the register a new workflow.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        [TestPriority((int)TestOrder.Register)]
        [Theory]
        [InlineData("test")]
        public async Task Calling_RegisterEndpoint_Result_In_New_Workflow_Entry(string environment)
        {
            // Arrange
            // Get the current manifest.
            var cdnClient = new CdnClient();
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment, true)}"), $"v5", $"{Config.ManifestEndPoint}");
            CurrentManifest = manifest;

            var client = new AppClient();

            // Register
            // Act
            var (responseMessage, enrollmentResponse) = await client.PostAsync<EnrollmentResponse>(new Uri($"{Config.AppBaseUrl(environment, true)}"), $"v2", $"{Config.RegisterEndPoint}", null);
            EnrollmentResponseV2 = enrollmentResponse;

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.NotNull(responseMessage.Headers.ToString().Split("padding: ")[1]); // Padding should be added in the header
            Assert.NotNull(enrollmentResponse.BucketId);
            Assert.NotNull(enrollmentResponse.ConfirmationKey);
            Assert.NotNull(enrollmentResponse.GGDKey);
        }

        [TestPriority((int)TestOrder.PostKeys)]
        [Theory]
        [InlineData("test")]
        public async Task Calling_PostKeysEndpoint_Result_In_New_Added_Teks(string environment)
        {
            var client = new AppClient();

            // Arrange
            PostTeks = GenerateTeks(EnrollmentResponseV2.BucketId);

            var tekDates = PostTeks.Keys
                .OrderBy(x => x.RollingStartNumber)
                .Select(x => new { x, Date = x.RollingStartNumber.FromRollingStartNumber() });

            foreach (var i in tekDates)
            {
                Trace.WriteLine($"RSN:{i.x.RollingStartNumber} Date:{i.Date:yyyy-MM-dd}.");
            }

            Trace.Write(PostTeks.ToJson());

            var data = PostTeks.ToByteArray();

            var signature = HttpUtility.UrlEncode(Sign(Convert.FromBase64String(EnrollmentResponseV2.ConfirmationKey), data));
            var postkeysContent = new ByteArrayContent(data);
            postkeysContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            // Act
            var postkeysResult = await client.PostAsync(new Uri($"{Config.AppBaseUrl(environment, true)}"), "v1", $"postkeys?sig={signature}", postkeysContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, postkeysResult.StatusCode);

            // Authorize
            // Arrange
            var publishTekArgs = new PublishTekArgs
            {
                GGDKey = EnrollmentResponseV2.GGDKey,
                SubjectHasSymptoms = true,
                DateOfSymptomsOnset = DateTime.Today
            };

            var pubtekContent = new StringContent(publishTekArgs.ToJson(), Encoding.UTF8, "application/json");

            // Act
            var pubTekResult = await client.PutAsync(new Uri($"{Config.IccApiBaseUrl(environment, true)}"), "pubtek", pubtekContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, pubTekResult.StatusCode);
        }

        [TestPriority((int)TestOrder.AuthorizePubTek)]
        [Theory]
        [InlineData("test")]
        public async Task CallingPubTekEndpoint_Result_In_Authorized_Entry(string environment)
        {
            var client = new AppClient();

            // Authorize
            // Arrange
            var publishTekArgs = new PublishTekArgs
            {
                GGDKey = EnrollmentResponseV2.GGDKey,
                SubjectHasSymptoms = true,
                DateOfSymptomsOnset = DateTime.Today
            };

            var pubtekContent = new StringContent(publishTekArgs.ToJson(), Encoding.UTF8, "application/json");

            // Act
            var pubTekResult = await client.PutAsync(new Uri($"{Config.IccApiBaseUrl(environment, true)}"), "pubtek", pubtekContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, pubTekResult.StatusCode);
        }

        [TestPriority((int)TestOrder.VerifyExposureKeySet)]
        [Theory]
        [InlineData("test")]
        public async Task New_ExposureKeySets_Should_Be_Published(string environment)
        {
            // Arrange
            // Get the current manifest.
            var cdnClient = new CdnClient();
            ManifestContent newManifest = null;

            var manifestIsNew = false;
            var maxRetries = 30;
            var retriesCount = 0;
            while (!manifestIsNew && retriesCount < maxRetries)
            {
                (_, newManifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment, true)}"), $"v5", $"{Config.ManifestEndPoint}");
                manifestIsNew = !newManifest.Equals(CurrentManifest);

                if (!manifestIsNew)
                {
                    retriesCount++;
                    await Task.Delay(30 * 1000); // Delay 30 seconds
                }
            }

            // Manifest should be new
            Assert.False(newManifest.Equals(CurrentManifest));

            // Act
            var (responseMessage, rcp) = await cdnClient.GetCdnEksContent(new Uri($"{Config.CdnBaseUrl(environment, true)}"), $"v5", $"{Config.ExposureKeySetEndPoint}/{newManifest.ExposureKeySets.Last()}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            // 150 are expected (50 posted and stuffed to 150 total)
            Assert.True(rcp.Count >= 150);

            var keys = PostTeks?.Keys.Select(p => p.KeyData).ToList();
            var result = rcp?.Where(x => keys.Contains(x)).ToList();

            Assert.Equal(PostTeks?.Keys.Length, result?.Count);
        }

        private PostTeksArgs GenerateTeks(string bucketId)
        {
            var keys = new List<PostTeksItemArgs>
            {
                // Add 1 key
                new PostTeksItemArgs
                {
                    // This test expects the keys to be published without prolonged embargo
                    RollingStartNumber = DateTime.UtcNow.Date.AddDays(-1).ToRollingStartNumber(),
                    RollingPeriod = 1,
                    KeyData = Convert.ToBase64String(_randomNumberGenerator.NextByteArray(UniversalConstants.DailyKeyDataByteCount))
                }
            };

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
