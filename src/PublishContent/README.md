# PublishContent

The PublishContent project is used to insert or update application configuration files into the [Content database](../Database/Content/dbo/Tables/Content.sql). These configuration files are used by the mobile apps to make certain functional decisions and provide the apps with (translated) resources.

The following configuration files exist:

* AppConfig.json
* ResourceBundle.json
* RiskCalculationParameters.json

The current production versions of these files can be found in the [Resources](Resources) folder in this project.

## AppConfig

TODO: add description

| Property | Description |
| -------- | ----------- |
| cachebuster | *deprecated; will be removed*|
| androidMinimumVersion | The minimum recommended app version for Android |
| appointmentPhoneNumber | The phone number to call the GGD to make an appointment to get tested for SARS-CoV-2|
| decoyProbability | |
| iOSAppStoreURL | |
| iOSMinimumVersion | The minimum recommended app version for iOS|
| iOSMinimumVersionMessage | |
| manifestFrequency | |
| repeatedUploadDelay | |
| requestMaximumSize | |
| requestMinimumSize | |
| shareKeyURL | The URL used to share Temporary Exposure Keys through [coronatest.nl](https://coronatest.nl/)|
| coronaTestURL | The URL used to make an appointment through [coronatest.nl](https://coronatest.nl/) to get tested for SARS-CoV-2|
| featureFlags: independentKeySharing | List of feature flags; currently only `independentKeySharing` is available, which toggles the ability to share Temporary Exposure Keys directly through [coronatest.nl](https://coronatest.nl/) on or off (i.e., without the need to call the GGD) |
| coronaMelderDeactivated | Boolean flag to toggle whether the CoronaMelder app is switched on or off (through string matching by the apps on the string `deactivated`)|
| coronaMelderDeactivatedTitle | |
| coronaMelderDeactivatedBody | |

## ResourceBundle

TODO: add description

## RiskCalculationParameters

TODO: add description

| Property | Description |
| -------- | ----------- |
| daysSinceOnsetToInfectiousness | A mapping between `daysSinceOnsetOfSymptoms` and `infectiousness`.<br> `daysSinceOnsetOfSymptoms`: how many days ago was the first day of SARS-CoV-2 symptoms <br> `infectiousness`: boolean value to indicate whether someone is considered infectious on the corresponding day |
| infectiousnessWhenDaysSinceOnsetMissing | |
| minimumWindowScore | |
| minimumRiskScore | |
| daysSinceExposureThreshold | |
| attenuationBucketThresholds | |
| attenuationBucketWeights | |
| infectiousnessWeights | |
| reportTypeWeights | |
| reportTypeWhenMissing | |
| cachebuster | *deprecated; will be removed*|
