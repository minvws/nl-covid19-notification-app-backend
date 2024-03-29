using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.E2ETests;
using Endpoint.Tests.ContentModels;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using Xunit;

namespace Endpoint.Tests
{
    [Trait("test", "e2e")]
    public class ContentTests : TestBase
    {
        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        [InlineData("test", "v5")]
        [InlineData("acc", "v5")]
        [InlineData("prod", "v5")]
        public async Task Should_HaveReceived_The_Manifest_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (responseMessage, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode); // I should have received the manifest
            Assert.NotNull(manifest); // Manifest retrieved.
            Assert.NotNull(manifest.AppConfig); // AppConfig key retrieved.
            Assert.NotNull(manifest.RiskCalculationParameters); // RiskCalculationParameters key retrieved.
            Assert.NotNull(manifest.ExposureKeySets); // ExposureKeySets key retrieved.
            Assert.NotNull(manifest.ResourceBundle); // ResourceBundle key retrieved.
        }

        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        [InlineData("test", "v5")]
        [InlineData("acc", "v5")]
        [InlineData("prod", "v5")]
        public async Task Should_HaveReceived_The_AppConfig_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");
            var (responseMessage, appConfig) = await cdnClient.GetCdnContent<AppConfig>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.AppConfigEndPoint}/{manifest.AppConfig}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(MediaTypeHeaderValue.Parse("application/zip"), responseMessage.Content.Headers.ContentType); // A appconfig response structure validation
            var maxFileLength = 1024 * 20; // 20KB
            Assert.True(responseMessage.Content.Headers.ContentLength < maxFileLength); // Validate max file size
            Assert.True(responseMessage.Headers.Age?.TotalSeconds < responseMessage.Headers.CacheControl.MaxAge?.TotalSeconds); // Max-Age of app config data validated, not older then 1209600 sec. (14 days)


            Assert.NotNull(appConfig); // AppConfig file retrieved.
            Assert.True(appConfig.androidMinimumVersion != 0);
            Assert.NotNull(appConfig.iOSMinimumVersion);
            Assert.NotNull(appConfig.iOSMinimumVersionMessage);
            Assert.NotNull(appConfig.iOSAppStoreURL);
            Assert.True(appConfig.manifestFrequency != 0);
            Assert.True(appConfig.decoyProbability != 0);
            Assert.True(appConfig.requestMinimumSize != 0);
            Assert.True(appConfig.requestMaximumSize != 0);
            Assert.True(appConfig.repeatedUploadDelay != 0);
        }

        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        [InlineData("test", "v5")]
        [InlineData("acc", "v5")]
        [InlineData("prod", "v5")]
        public async Task Should_HaveReceived_The_RiskCalculationParameters_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");
            var (responseMessage, rcp) = await cdnClient.GetCdnContent<RiskCalculationParameters>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.RiskCalculationParametersEndPoint}/{manifest.RiskCalculationParameters}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(MediaTypeHeaderValue.Parse("application/zip"), responseMessage.Content.Headers.ContentType); // A appconfig response structure validation
            var maxFileLength = 1024 * 20; // 20KB
            Assert.True(responseMessage.Content.Headers.ContentLength < maxFileLength); // Validate max file size
            Assert.True(responseMessage.Headers.Age?.TotalSeconds < responseMessage.Headers.CacheControl.MaxAge?.TotalSeconds); // Max-Age of app config data validated, not older then 1209600 sec. (14 days)


            Assert.NotNull(rcp); // AppConfig file retrieved.
            Assert.True(rcp.daysSinceOnsetToInfectiousness != null);
            Assert.Equal(29, rcp.daysSinceOnsetToInfectiousness.Length); // -14 to -1, 0 and 1 to 14 makes 29
            Assert.True(rcp.attenuationBucketThresholds != null);
            Assert.Equal(3, rcp.attenuationBucketThresholds.Length); // low mid high
            Assert.True(rcp.attenuationBucketWeights != null);
            Assert.True(rcp.infectiousnessWeights != null);
            Assert.True(rcp.reportTypeWeights != null);
        }

        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        [InlineData("test", "v5")]
        [InlineData("acc", "v5")]
        [InlineData("prod", "v5")]
        public async Task Should_HaveReceived_The_ResourceBundle_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");
            var (responseMessage, resourceBundle) = await cdnClient.GetCdnContent<ResourceBundle>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ResourceBundleEndPoint}/{manifest.ResourceBundle}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(MediaTypeHeaderValue.Parse("application/zip"), responseMessage.Content.Headers.ContentType); // A appconfig response structure validation
            Assert.True(responseMessage.Headers.Age?.TotalSeconds < responseMessage.Headers.CacheControl.MaxAge?.TotalSeconds); // Max-Age of app config data validated, not older then 1209600 sec. (14 days)

            Assert.NotNull(resourceBundle); // AppConfig file retrieved.
            resourceBundle.CreateTestableResourceBundle();

            Assert.Equal(11, resourceBundle.TestableResources.Count);
        }

        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        [InlineData("test", "v5")]
        [InlineData("acc", "v5")]
        [InlineData("prod", "v5")]
        public async Task Should_HaveReceived_The_ExposureKeySet_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");
            var (responseMessage, rcp) = await cdnClient.GetCdnEksExport(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ExposureKeySetEndPoint}/{manifest.ExposureKeySets.Last()}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(MediaTypeHeaderValue.Parse("application/zip"), responseMessage.Content.Headers.ContentType); // A appconfig response structure validation
            Assert.True(responseMessage.Headers.Age?.TotalSeconds < responseMessage.Headers.CacheControl.MaxAge?.TotalSeconds); // Max-Age of app config data validated, not older then 1209600 sec. (14 days)

            Assert.NotNull(rcp);

            Assert.True(!string.IsNullOrEmpty(rcp.StartTimestamp.ToString()));
            Assert.True(!string.IsNullOrEmpty(rcp.EndTimestamp.ToString()));
            Assert.True(!string.IsNullOrEmpty(rcp.Region));
            Assert.True(rcp.BatchNum > 0);
            Assert.True(rcp.BatchSize > 0);
            Assert.True(rcp.SignatureInfos.Any());
            Assert.True(rcp.Keys.Any());

            Assert.True(rcp.SignatureInfos.First().SignatureAlgorithm.Equals("1.2.840.10045.4.3.2"));
            Assert.True(rcp.SignatureInfos.First().VerificationKeyId.Equals("204"));
            Assert.True(!string.IsNullOrEmpty(rcp.SignatureInfos.First().VerificationKeyVersion));

            foreach (var temporaryExposureKey in rcp.Keys)
            {
                Assert.True(temporaryExposureKey.HasKeyData);
                Assert.True(temporaryExposureKey.HasRollingStartIntervalNumber);
                Assert.True(temporaryExposureKey.HasRollingPeriod);
                Assert.True(temporaryExposureKey.HasDaysSinceOnsetOfSymptoms);

                Assert.NotNull(temporaryExposureKey.KeyData);
                Assert.True(temporaryExposureKey.RollingStartIntervalNumber > 0);
                Assert.True(temporaryExposureKey.RollingPeriod > 0);
                Assert.True(!string.IsNullOrEmpty(temporaryExposureKey.DaysSinceOnsetOfSymptoms.ToString()));
            }
        }
    }
}
