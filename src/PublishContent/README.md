# PublishContent

The PublishContent project is used to insert or update application configuration files into the [Content database](../Database/Content/dbo/Tables/Content.sql). These configuration files are used by the mobile apps to make certain functional decisions and provide the apps with (translated) resources.

The following configuration files exist:

* AppConfig.json
* ResourceBundle.json
* RiskCalculationParameters.json

The current production versions of these files can be found in the [Resources](Resources) folder in this project.

## AppConfig

File containing configuration values for the behavior of the apps themselves, such as frequency of checks and minimum app versions.

| Property | Description |
| -------- | ----------- |
| cachebuster | *deprecated; will be removed* |
| androidMinimumVersion | The minimum recommended app version for Android |
| appointmentPhoneNumber | The phone number to call the GGD to make an appointment to get tested for SARS-CoV-2 |
| decoyProbability | Probability used in calculating whether or not a decoy upload should be performed. Allowed values 0 ≥ P ≤ 1 |
| iOSAppStoreURL | Contains the link to the CoronaMelder app store page. "https://apps.apple.com/nl/app/id1517652429" |
| iOSMinimumVersion | The minimum recommended app version for iOS |
| iOSMinimumVersionMessage | The text that can be displayed on the minimumVersion screen. If not filled the app will fall back to the local version of the text |
| manifestFrequency | Number of minutes after which the app will (automatically) refresh the manifest |
| repeatedUploadDelay | |
| requestMaximumSize | |
| requestMinimumSize | |
| shareKeyURL | The URL used to share Temporary Exposure Keys through [coronatest.nl](https://coronatest.nl/)|
| coronaTestURL | The URL used to make an appointment through [coronatest.nl](https://coronatest.nl/) to get tested for SARS-CoV-2|
| featureFlags: independentKeySharing | List of feature flags; currently only `independentKeySharing` is available, which toggles the ability to share Temporary Exposure Keys directly through [coronatest.nl](https://coronatest.nl/) on or off (i.e., without the need to call the GGD) |
| coronaMelderDeactivated | Boolean flag to toggle whether the CoronaMelder app is switched on or off (through string matching by the apps on the string `deactivated`)|
| coronaMelderDeactivatedTitle | Contains reference to the correct string in the resourceBundle. The value should be "end_of_life_title" |
| coronaMelderDeactivatedBody | Contains reference to the correct string in the resourceBundle. The value should be "end_of_life_body" |

The following property can also exist in the AppConfig, but is currently unused:
```
"notification": {
  "scheduledDateTime": "2022-04-20T21:00:00+01:00",
  "title": "notification_app_deactivation_title",
  "body": "notification_app_deactivation_body",
  "targetScreen": "main",
  "probability": 1
}
```
This `notification` property allows for configuring a push notification to be sent to the mobile apps at the configured `scheduledDateTime`, with the given `title` and `body`. The `targetScreen` sub-property defines where users will land in the app when they click the notification, the `probability` sub-property allows for targeting only a given portion of users with the push notification. A `probability` with a value of 1 sends the configured push notification to all users.

## ResourceBundle

File containing the textual resources for the apps, including corresponding translations. This allows for changing for instance the treatment perspective when government advice changes, without having to do a full app release.

## RiskCalculationParameters

File containing configuration values for determining the risk associated with a given exposure.

| Property | Description |
| -------- | ----------- |
| daysSinceOnsetToInfectiousness | A mapping between `daysSinceOnsetOfSymptoms` and `infectiousness`.<br> `daysSinceOnsetOfSymptoms`: how many days ago was the first day of SARS-CoV-2 symptoms <br> `infectiousness`: boolean value to indicate whether someone is considered infectious on the corresponding day |
| infectiousnessWhenDaysSinceOnsetMissing | *not used in our appConfig* <br> Default `infectiousness` value when the diagnosis keys don't include the `daysSinceOnsetOfSymptoms` field |
| minimumWindowScore | The minimum score that is needed before a scanWindow will be included in the minimumRiskScore calculation. A scanWindow can consist of one or more ScanInstance; i.e., one continuous measurement of BLE-signal broadcasted by another GAEN app user |
| minimumRiskScore | The minimum score that is needed before an exposure notification is sent. This minimum score can be reached based on one (depending on the values it is set to) or more scanWindows |
| daysSinceExposureThreshold | *not used in our appConfig* |
| attenuationBucketThresholds | Thresholds defining the BLE attenuation buckets edges. This list must have 3 elements: the immediate, near, and medium thresholds |
| attenuationBucketWeights | Duration weights to associate with ScanInstances depending on the attenuation bucket in which their typicalAttenuation falls. <br> This list must have four elements, corresponding to the weights for the following buckets:<br><ul><li>Immediate bucket:<br>-infinity < attenuation <= immediate threshold</li><li>Near bucket:<br>immediate threshold < attenuation <= near threshold</li><li>Medium bucket:<br>near threshold < attenuation <= medium threshold</li><li>Other bucket:<br>medium threshold < attenuation < +infinity</li></ul> |
| infectiousnessWeights | A map that stores a weight for each possible value of infectiousness |
| reportTypeWeights | A map that stores a weight for each possible value of reportType |
| reportTypeWhenMissing | Default ReportType value when the diagnosis keys don't include it |
| cachebuster | *deprecated; will be removed*|
