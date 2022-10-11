# COVID-19 Notification App - Backend

## Table of Contents
[Introduction](#Introduction)  
[External Documentation](#External-Documentation)  
[Development and Contribution Process](#Development-and-Contribution-Process)  
[Local Development Setup](#Local-Development-Setup)  
[License](#License)  
[Attribution](#Attribution)  
[Unreleased](#unreleased)   

## Introduction

This repository contains the backend code for the Dutch exposure notification app.

- The backend is located in the repository you are currently viewing.
- The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
- The Android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
- The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
- The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination
- The HSM Signer Service used to sign content can be found here: https://github.com/minvws/nl-rdo-hsm-signer-service

The backend code runs on .NET 6.

## External Documentation

The Dutch exposure notification app uses the Google Apple Exposure Notification (GAEN) framework developed by Google and Apple as part of their effort to help combat the SARS-CoV-2 pandemic. Please find their documentation in one of the following 2 locations:

- [Google's Android Exposure Notifications Implementation Guide](https://developers.google.com/android/exposure-notifications/implementation-guide)
- [Apple's iOS Exposure Notification Documentation](https://developer.apple.com/documentation/exposurenotification)

The Dutch exposure notification app is part of the group of EU countries using the European Federation Gateway Service (EFGS) for sharing their national exposure keys on a European level. Please find the EFGS code and documentation on GitHub:

- [efgs-federation-gateway](https://github.com/eu-federation-gateway-service/efgs-federation-gateway)

## Development and Contribution Process

The core team works on the repository in a private fork for reasons of compliance with existing processes, and will share its work as often as possible.

If you plan to make code changes, please feel free to open an issue where we can discuss your changes, before opening a pull request. This avoids possibly doing work that we might not be able to use due to various reasons (specific infrastructure demands, already working on, etc).

If you think the information contained in this README is incomplete or wrong, please feel free to directly open a pull request on the README.

If you have any other questions about the README or the information contained therein, please feel free to open an issue.

## Local Development Setup

Before being able to run the projects contained in the backend solution, you will need to set up a database, and be able to connect to the [HSM Signer Service](https://github.com/minvws/nl-rdo-hsm-signer-service) test server.

### Certificates

CoronaMelder has its files signed by the [HSM Signer Service](https://github.com/minvws/nl-rdo-hsm-signer-service) with a RSA certificate and an ECDSA certificate. The latter is a requirement set by Apple and Google.

Versions of these certificates for local testing can be found in the folder `src/Crypto/Resources`:
- TestRSA.p12  
- TestECDSA.p12  
  
**Please note: these certificates are not production certificates.**

The file password for TestRSA.p12 is `Covid-19!`; the password for TestECDSA.p12 is empty.

The file `StaatDerNLChain-EV-Expires-2022-12-05.p7b` can be ignored, as the local certificates are self-signed.

### Database

This project assumes the presence of a PostgreSQL database.

After installing PostgreSQL, you can either create all the necessary databases and tables manually (using the SQL scripts contained in the [databases](databases) directory), or run the `DbProvision` project to have everything generated automatically.

### Projects
The codebase consists of the following projects, allowing you to locally set up a backend that contains Temporary Exposure Keys, Exposure Key Sets, a Manifest, and the various configuration JSON files that are representative of the actual backend.  

#### DbProvision
A console application that removes and rebuilds the required databases; only used for development and debugging.  
The `nonuke`-argument can be supplied to prevent removing any existing databases.  
Additionally, several types of JSON files can be inserted into the database by means of passing one of the following arguments, followed by a path to the specific JSON file:
- `-a`, for Appconfig.json
- `-r`, for RiskCalculationParameters.json
- `-b`, for ResourceBundle.json

#### GenTeks
A console application that generates Temporary Exposure Keys ('TEKs') and inserts them into the database. For development and testing purposes only.  
Two arguments can be passed to the application: the amount of workflows (or 'buckets') and the amount of TEKs per workflow.  
By default, 10 workflows with each 1000 TEKs are created, equivalent to passing the arguments `10 1000`. These arguments can be found (and changed) in `src/GenTeks/Properties/launchSettings.json`

#### ForceTekAuth
A console application that authenticates all workflows in the database, equivalent to users contacting the GGD to publish their workflows, or self-authorising their workflows through [coronatest.nl](https://coronatest.nl/).

This project is for development and testing purposes only. No command-line arguments can be provided.

As the GenTeks project currently marks workflows as "authorised" when it creates them, there is strictly speaking no need to run the ForceTekAuth project anymore.

#### PublishContent
A console application that, equal to DbProvision, allows the insertion of various JSON files into the database.  
The files can be inserted into the database by means of passing one of the following arguments, followed by a path to a JSON file that contains said content:
- `-a`, for Appconfig.json
- `-r`, for RiskCalculationParameters.json
- `-b`, for Resourcebundle.json

#### SigTestFileCreator (deprecated)

_Deprecated: the backend no longer self-signs content, so installing certificates is no longer necessary_

A console application that is used to check if the private keys of the installed certificates can be accessed and used. For development and testing purposes only.

The program has one command-line argument: the path to a file that will be signed with the RSA and GAEN key.

#### ProtobufScrubber (deprecated)

_Deprecated: the backend no longer self-signs content, and the `X962PackagingFix.cs` is no longer used_

A console application that alters the GAEN signature of a signed file so that it can be verified by OpenSSL. The requirement of this program stems from a difference in writing the header bytes to the signature files. See `X962PackagingFix.cs` for more info.  

The program has one command-line argument: the path to a zip with an `export.sig` file.

#### EfgsTestDataGenerator
A webservice that is used for automated end-to-end tests. It exposes one endpoint to mock the EFGS-service.  

#### Content.WebApi
A webservice for downloading the various files by the clients. It exposes endpoints that allows the user to download the manifests, exposure keysets, appconfigs, resourcebundles and risk calculation parameters.

#### DailyCleanup
A console application that removes old data (i.e., older than 14 days) from the database, in order to comply with privacy regulations.

After the cleanup, it generates a set of statistics:  
- The total amount of TEKs
- The amount of published TEKs
- The total amount of Workflows
- The total amount of Workflows with TEKs
- The total amount of Workflows authorised

As the name suggests, the DailyCleanup is run every night at a set time.

#### EksEngine
A console application that takes all authorised workflows and downloaded TEKs from EFGS, bundles them into an EKS, signs it and places it in the databas. It then updates the manifest and prepares the TEKs from the authorised workflows for delivery to EFGS. Please find more elaborate documentation on the EksEngine in `docs`.

#### Icc.V2.WebApi
A webservice that exposes the PubTek-endpoint. This endpoint is used to authorise workflows with the GGD key. Its functionality is used by the GGD Portal and by [coronatest.nl](https://coronatest.nl/).

#### Icc.WebApp
This project contains 2 functionalities:
1. the frontend code for the GGD Portal, utilizing [Microsoft.AspNetCore.SpaServices.Extensions](https://www.nuget.org/packages/Microsoft.AspNetCore.SpaServices.Extensions).
2. a webservice that is used as a passthrough to the PubTek-endpoint described [above](#Icc.V2.WebApi) for the GGD Portal, and that provides authorisation functionality for the GGD Portal.

#### Iks.Downloader
A console application that downloads TEKs from EFGS and stores them to be processed by the EKSEngine.

#### Iks.Uploader
A console application that takes the TEKs that the EKSEngine has prepared and uploads them to EFGS.

#### ManifestEngine
A console application that updates the manifest with the most recent content of the databases.

#### MobileAppApi.WebApi
The webservice that the clients use to request the creation of a Workflow (or 'bucket'), upload their TEKs and perform decoy uploads to preserve the privacy of the client.

## License

This project is licensed under the EUPL license. See [LICENSE](LICENSE/LICENSE.txt) for more information.

## Attribution

Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).

## Unreleased
The `feature/dashboarddata` branch contains the backend code changes for the (as of yet unreleased) in-app COVID-19 related statistics (similar to [coronadashboard.rijksoverheid.nl](https://coronadashboard.rijksoverheid.nl)).

The new functionalities added to the backend are:
- fetching of data with the DashboardData.Downloader
- making the downloaded data available for the apps through a new API endpoint `v1/dashboard` in Content.WebApi
- removing old downloaded data with the DailyCleanup process

The status of this branch is "work in progress", and additional improvements are necessary to make it more robust.

For an extensive explanation of the dashboard functionality, please see one of the following pages for more information:

- Android: [CORONA_DASHBOARD.md](https://github.com/minvws/nl-covid19-notification-app-android/blob/release/2.6.0-dashboard/CORONA_DASHBOARD.md)
- iOS: [CORONA_DASHBOARD.md](https://github.com/minvws/nl-covid19-notification-app-ios/blob/feature/coronamelder-dashboard/CORONA_DASHBOARD.md)
