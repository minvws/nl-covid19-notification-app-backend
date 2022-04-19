# COVID-19 Notification App - Backend

## Introduction

This repository contains the backend code for the Dutch exposure notification app.

* The backend is located in the repository you are currently viewing.
* The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
* The Android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
* The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
* The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination

The backend code runs on .NET Core 3.1. End of support for this version is December 3rd, 2022.

## Development & Contribution process

The core team works on the repository in a private fork for reasons of compliance with existing processes, and will share its work as often as possible.

If you plan to make changes, please feel free to open an issue beforehand where we can discuss your changes. This avoids possibly doing work that we might not be able to use due to various reasons (specific infrastructure demands, already working on, etc).

## Local development setup
Before being able to run the projects contained in the backend solution, you will need to set up a database, and install a test certificate on the machine that will run the code.

### Certificates (this section is to be expanded)
This solution contains the following certificates, located in `src/Crypto/Resources`:
- StaatDerNLChain-EV-Expires-2022-12-05.p7b
- TestECDSA.p12
- TestRSA.p12
- BdCertChain.p7b (deprecated)

For local development, you will need to install the `TestRSA.p12` certificate on your local machine.

This project assumes the RSA certicicate is installed in the Local Machine Certificate Store.

For macOS this means the project assumes the RSA certificate is installed in the @@@ keychain. Please note that this makes running the project locally slightly awkward, as it involves either giving the code permission to access this keychain indefinitely, or otherwise forces the developer to click "Allow" a large amount of times. To ger around this, please make the following changes:

@@@

**Please note: these certificates are not production certificates.**

### Database
This project assumes the presence of a Microsoft SQL Server database.

For local development on Windows, it would suffice to download [SQL Server Developer](https://www.microsoft.com/nl-nl/sql-server/sql-server-downloads).

For local development on macOS or Linux, local installation of SQL Server is not possible, and as such we have created a small Docker setup that contains a database to make developing locally on macOS and Linux possible. Of course you can also use this on Windows if you do not want to install SQL Server on your machine.

### Docker (general)
To start a local development environment you can use docker-compose:
```bash
# Solution root
cd docker
docker-compose up --build
```
### Docker (macOS M1)
The Docker image used will currently not work out of the box for macOS machines with ARM architecture (macOS M1). To use the Docker setup on macOS M1, please make the following changes:

@@@

### Projects
This code base consists of the following projects that allow you to locally set up a backend that contains Temporary Exposure Keys, Exposure Key Sets, a Manifest, and the various configuration JSON files that are representative of the actual backend.

#### GenTeks
This console application project generates Temporary Exposure Keys (or TEKs) and inserts them into the database. This project exists for development and testing purposes only. The amount of TEKs generated can be controlled by `commandLineArgs` in `launchSettings.json` (located in `src/GenTeks/Properties/`). The default setting is `10 1000`, which means 10 so-called workflows (also knows as buckets) are created, with 1000 TEKs in each workflow/bucket.

## Attribution

Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).
