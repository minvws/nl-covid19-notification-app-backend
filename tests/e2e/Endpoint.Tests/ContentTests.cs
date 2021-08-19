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
    public class ContentTests : TestBase
    {
        [Theory]
        [InlineData("test", "v3")]
        [InlineData("acc", "v3")]
        [InlineData("prod", "v3")]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        public async Task Should_HaveReceived_The_Manifest_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (responseMessage, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");

            // Assert
            Assert. Equal(HttpStatusCode.OK, responseMessage.StatusCode); // I should have received the manifest
            Assert.NotNull(manifest); // Manifest retrieved.
            Assert.NotNull(manifest.AppConfig); // AppConfig key retrieved.
            Assert.NotNull(manifest.RiskCalculationParameters); // RiskCalculationParameters key retrieved.
            Assert.NotNull(manifest.ExposureKeySets); // ExposureKeySets key retrieved.
            Assert.NotNull(manifest.ResourceBundle); // ResourceBundle key retrieved.
        }

        [Theory]
        [InlineData("test", "v3")]
        [InlineData("acc", "v3")]
        [InlineData("prod", "v3")]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
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

            foreach (var resource in resourceBundle.TestableResources)
            {
                Assert.True(!string.IsNullOrEmpty(resource.about_this_notification_body));
                Assert.True(!string.IsNullOrEmpty(resource.about_this_notification_title));
                Assert.True(!string.IsNullOrEmpty(resource.advice_body));
                Assert.True(!string.IsNullOrEmpty(resource.advice_title));
                Assert.True(!string.IsNullOrEmpty(resource.dont_test_body));
                Assert.True(!string.IsNullOrEmpty(resource.dont_test_title));
                Assert.True(!string.IsNullOrEmpty(resource.exposure_notification_body));
                Assert.True(!string.IsNullOrEmpty(resource.exposure_notification_body_exposure_days_11_x));
                Assert.True(!string.IsNullOrEmpty(resource.exposure_notification_body_v1_legacy));
                Assert.True(!string.IsNullOrEmpty(resource.exposure_notification_title));
                Assert.True(!string.IsNullOrEmpty(resource.medical_help_body));
                Assert.True(!string.IsNullOrEmpty(resource.medical_help_title));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_body));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_body_exposure_days_11_x));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_body_exposure_days_1_10));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_body_exposure_days_4_10));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_body_exposure_days_x_3));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_title));
                Assert.True(!string.IsNullOrEmpty(resource.next_steps_title_exposure_days_11_x));
                Assert.True(!string.IsNullOrEmpty(resource.stay_home_body));
                Assert.True(!string.IsNullOrEmpty(resource.stay_home_title));
                Assert.True(!string.IsNullOrEmpty(resource.symptoms_body));
                Assert.True(!string.IsNullOrEmpty(resource.symptoms_title));
                Assert.True(!string.IsNullOrEmpty(resource.test_negative_body));
                Assert.True(!string.IsNullOrEmpty(resource.test_negative_title));
                Assert.True(!string.IsNullOrEmpty(resource.vaccinated_body));
                Assert.True(!string.IsNullOrEmpty(resource.vaccinated_title));
            }
        }

        [Theory]
        [InlineData("test", "v4")]
        [InlineData("acc", "v4")]
        [InlineData("prod", "v4")]
        public async Task Should_HaveReceived_The_ExposureKeySet_With_Correct_Values(string environment, string version)
        {
            // Arrange
            var cdnClient = new CdnClient();

            // Act
            var (_, manifest) = await cdnClient.GetCdnContent<ManifestContent>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ManifestEndPoint}");
            var (responseMessage, rcp) = await cdnClient.GetCdnContent<ExposureKeySet>(new Uri($"{Config.CdnBaseUrl(environment)}"), $"{version}", $"{Config.ExposureKeySetEndPoint}/{manifest.ExposureKeySets.First()}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.Equal(MediaTypeHeaderValue.Parse("application/zip"), responseMessage.Content.Headers.ContentType); // A appconfig response structure validation
            Assert.True(responseMessage.Headers.Age?.TotalSeconds < responseMessage.Headers.CacheControl.MaxAge?.TotalSeconds); // Max-Age of app config data validated, not older then 1209600 sec. (14 days)


            Assert.NotNull(rcp);
        }
    }
}
