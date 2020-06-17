# Covid19 Notification App Backend

## Introduction
This repository contains the backend code for the Proof of Concept for the Dutch exposure notification app. We provide this code in an early stage so that everyone can follow along as we develop the app, and to solicit feedback. Note that due to the early stage in which we are sharing this code, significant portions of the code might still change. We might add or remove features and code as needed, based on validation and user tests that are conducted partially in parallel to the development.

* The backend is located in the repository you are currently viewing.
* The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
* The android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
* The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
* The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination

## Development & Contribution process

The core team works on the repository in a private fork (for reasons of compliance with existing processes) and will share its work as often as possible.

If you plan to make non-trivial changes, we recommend to open an issue beforehand where we can discuss your planned changes.
This increases the chance that we might be able to use your contribution (or it avoids doing work if there are reasons why we wouldn't be able to use it).

## Running a standalone server

Local development support is provided for all platforms - Windows, macOS, Linux - by ServerStandAlone - a .NET Core MVC Web App, and the command line apps in the Data Utilites folder.

## Development Tools

* Visual Studio 2019
* Your choice of SQL Server instances

## Supporting local mobile app development

### Standalone

1. Download dotnet SDK: https://dotnet.microsoft.com/download/dotnet-core/3.1
1. Clone this repo
1. Setup a database instance. Windows users can use a local SQL Server.
1. Add an `appsettings.Development.json` file. This overrides the settings in appsettings.json. And add a value for the MSS connection string.
1. Go to the ServerStandAlone folder and run it with 'dotnet run'.

### Docker
To quickly start a Standalone development environment you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up --build
``` 

See the individual app folders for details.

## Attribution
Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).

### Migrations
1. Set Startup project to DatabaseProvisioningTool
1. Open Package Manager Console
1. add-migration InitialCreate -Context ExposureContentDbContext -OutputDir Migrations/ExposureContentDbContext
