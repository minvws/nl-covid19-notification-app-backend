# COVID-19 Notification App - Backend

## Table of Contents
[Introduction](#introduction)<br>
[External Documentation](#external-documentation)<br>
[Development and Contribution Process](#development-and-contribution-process)<br>
[Local Development Setup](#local-development-setup)<br>
[Attribution](#attribution)<br>

## Introduction

This repository contains the backend code for the Dutch exposure notification app.

* The backend is located in the repository you are currently viewing.
* The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
* The Android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
* The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
* The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination

The backend code runs on .NET Core 3.1. End of support for this version of .NET is December 3rd, 2022.

## External Documentation
The Dutch exposure notification app uses the Google Apple Exposure Notification (GAEN) framework developed by Google and Apple as part of their effort to help combat the SARS-CoV-2 pandemic. Please find their documentation in one of the following 2 locations:

* [Google's Android Exposure Notifications Implementation Guide](https://developers.google.com/android/exposure-notifications/implementation-guide)
* [Apple's iOS Exposure Notification Documentation](https://developer.apple.com/documentation/exposurenotification)

The Dutch exposure notification app is part of the group of EU countries using the European Federation Gateway Service (EFGS) for sharing their national exposure keys on a European level. Please find the EFGS code and documentation on GitHub:

* [efgs-federation-gateway](https://github.com/eu-federation-gateway-service/efgs-federation-gateway)

## Development and Contribution Process

The core team works on the repository in a private fork for reasons of compliance with existing processes, and will share its work as often as possible.

If you plan to make code changes, please feel free to open an issue where we can discuss your changes, before opening a pull request. This avoids possibly doing work that we might not be able to use due to various reasons (specific infrastructure demands, already working on, etc).

If you think the information contained in this README is incomplete or wrong, please feel free to directly open a pull request on the README.

If you have any other questions about the README or the information contained therein, please feel free to open an issue.

## Local Development Setup
Before being able to run the projects contained in the backend solution, you will need to set up a database, and install a test certificate on the machine that will run the code.

### Certificates

CoronaMelder signs its files with an RSA-certificate and an ECDSA-certificate. The latter is a requirement set by Apple and Google.  
Versions of these certificates for local testing can be found in the folder `src/Crypto/Resources`:
- TestRSA.p12  
- TestECDSA.p12  
  
**Please note: these certificates are not production certificates.**  
The file-password for TestRSA.p12 is `Covid19!`; the password for TestECDSA.p12 is `12345678`.

The files `StaatDerNLChain-EV-Expires-2022-12-05.p7b` and `BdCertChain.p7b` can be ignored, as the local certificates are self-signed.  

#### Installation: Windows
Both certificates need to be installed into the local machine certificate store, under 'personal certificates'. Run `certlm.msc` to view this store.  

#### Installation: macOS  
For macOS the project assumes that the RSA certificate is installed in the *System* keychain. Please note that installing the certificate in the *System* keychain makes running the project locally slightly awkward, as it involves either giving the code permission to access this keychain permanently, or otherwise forces the developer to click "Allow" a large amount of times. To get around this, please make the following changes if you are running the backend on macOS:

1. Install the `TestRSA.p12` certificate in the *login* keychain.
2. Change `LocalMachineStoreCertificateProvider.cs` to read from `StoreLocation.CurrentUser` instead of `StoreLocation.LocalMachine`.

#### Installation: Linux  
TBD

### Database
This project assumes the presence of a Microsoft SQL Server database.

#### Windows
For local development on Windows, it would suffice to download [SQL Server Developer](https://www.microsoft.com/nl-nl/sql-server/sql-server-downloads).

After installing SQL Server, you can either create all the necessary databases and tables manually, or run the `DbProvision` project to have everything generated automatically.

#### macOS/Linux
For local development on macOS or Linux, local installation of SQL Server is not possible, and as such we have created a small Docker setup that contains a database to make developing locally on macOS and Linux possible. The Docker setup also contains `DbBuilder`, which serves the same function as the `DbProvision` project mentioned in the paragraph above. When combined together through `docker-compose`, you will end up with a database server populated with the necessary databases and tables running on Docker.

Of course you can also use the Docker setup on Windows if you do not want to install SQL Server on your machine.

### Docker (Windows/Linux/macOS)
To start the Docker database setup, you can use docker-compose:
```bash
# Solution root
cd docker
docker-compose up --build
```
This will create a Docker-based database server, as well as generate all the necessary databases and tables.

### Docker (macOS M1)
The Docker image used will currently not work out of the box for macOS machines with ARM architecture (macOS M1). To use the Docker setup on macOS M1, please make the following changes:

In `docker-compose.yml`, change
```
image: mcr.microsoft.com/mssql/server:2019-latest
```
to
```
image: mcr.microsoft.com/azure-sql-edge
```
The rest of the setup should work as-is.

### Projects
This code base consists of the following projects that allow you to locally set up a backend that contains Temporary Exposure Keys, Exposure Key Sets, a Manifest, and the various configuration JSON files that are representative of the actual backend.

#### GenTeks
This console application project generates Temporary Exposure Keys (or TEKs) and inserts them into the database. This project exists for development and testing purposes only. The amount of TEKs generated can be controlled by `commandLineArgs` in `launchSettings.json` (located in `src/GenTeks/Properties/`). The default setting is `10 1000`, which means 10 so-called workflows (also knows as buckets) are created, with 1000 TEKs in each workflow/bucket.

## License
This project is licensed under the EUPL license. See [LICENSE](LICENSE/LICENSE.txt) for more information.

## Attribution

Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).
