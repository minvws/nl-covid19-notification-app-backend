# COVID-19 Notification App - Backend

## Introduction

This repository contains the backend code for the Dutch exposure notification app.

* The backend is located in the repository you are currently viewing.
* The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
* The Android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
* The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
* The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination

## Development & Contribution process

The core team works on the repository in a private fork (for reasons of compliance with existing processes) and will share its work as often as possible.

If you plan to make non-trivial changes, we recommend to open an issue beforehand where we can discuss your planned changes.
This increases the chance that we might be able to use your contribution (or it avoids doing work if there are reasons why we wouldn't be able to use it).

## Running a standalone server

Local development support is provided for all platforms - Windows, macOS, Linux - by ServerStandAlone - a .NET Core MVC Web App, and the command line apps in the Data Utilities folder.

## Development Tools
* Visual Studio 2019 (or Rider)
* Your choice of SQL Server instances
* Or quickstart: only Docker

### Docker – to quickstart local app development

To quickly start a local Standalone development environment for app testing purposes you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up --build
``` 
**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptation or production environments.**

## Supporting local mobile app development

First make sure that you have the following installed:

1. Dotnet Core 3.1 SDK: https://dotnet.microsoft.com/download/dotnet-core/3.1
1. Node JS 12.18.1+ with NPM: https://nodejs.org/en/
1. Yarn: https://yarnpkg.com/
1. Angular CLI: https://angular.io/guide/setup-local
1. Optionally either Visual Studio or Rider.
1. A terminal is recommended, if you're running an older windows then https://cmder.net/ is useful. You need gitbash.

Then clone this repo.

### Standalone

1. Setup a database instance. Windows users can use a local SQL Server.
1. Add an `appsettings.Development.json` file. This overrides the settings in appsettings.json. And add a value for the MSS connection string.
1. Go to the ServerStandAlone folder and run it with 'dotnet run'.


### ICC Portal

The ICC Portal consists of a .Net Core backend found under `IccBackend` and an Angular / ASP.Net MVC Core frontend found under `ICCPortal`

#### ICC Portal backend

1. Setup a database instance. Windows users can use a local SQL Server.
1. Add an `appsettings.Development.json` file to the folder `IccBackend`. This overrides the settings in appsettings.json. And add a value for the MSS connection string.
1. Go to the `\IccBackend` folder and run it with `dotnet run`, this will start the backend in Kestrel.
1. Access the APIs here: `http://localhost:5000/swagger/index.html`.
1. To provision the database you must execute `​/devops​/nukeandpavedb` in Swagger.
1. Done :)

#### ICC Portal frontend

1. Clone this repo
1. Open a terminal and go to `\ICCPortal\ClientApp`.
1. Run `yarn` to install all of the dependencies.
1. Run the frontend with `ng serve`.
1. The server API url can be configured in `ICCPortal\ClientApp\src\environments\environment.ts`
1. Done :)

## Building and packaging

The build and package pipeline for this project is implemented as a set of batch scripts. These scripts can be executed on a development machine without arguments or via a build server (Jenkins, Azure Devops) without the need to lock-in the build process to one specific vendor. This should also make it easier to build older versions of the server because all of the logic required to build is committed to version control.

To run the scripts you must have following installed and accessible in your path:

* 7zip

Here is an overview of our scripts:

| Name                 | Description                                |
| -------------------- | ------------------------------------------ |
| build.bat            | Builds all of the project in release mode  |
|                      | and publishes packages into the folder     |
|                      | `publish` in the root of the solution.     |
| package.bat          | Packages the published files as zip        |
|                      | archives and puts them in `publish`.       |

## Attribution

Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).
