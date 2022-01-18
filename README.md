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

### Docker – to quickstart local app development

To quickly start a local Standalone development environment for app testing purposes you can use docker-compose:
```bash
# Solution root
cd docker
docker-compose up --build
``` 
**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptance or production environments.**


### ICC Portal (SPA Web Application)

The ICC Portal consists of a .Net Core hosted Angular frontend found under `Icc.WebApp`
1. Go to the `Icc.v2.WebApi` folder and run it with `dotnet run`, this will start the backend in Kestrel.
1. Access the APIs here: `http://localhost:5011/`.
1. Done :)

#### ICC Portal Pubtek Api

1. Setup a database instance. Windows users can use a local SQL Server.
1. Add an `appsettings.Development.json` file to the folder `Icc.v2.WebApi`. This overrides the settings in appsettings.json. And add a value for the MSS connection string.
1. Go to the `Icc.v2.WebApi` folder and run it with `dotnet run`, this will start the backend in Kestrel.
1. Access the APIs here: `http://localhost:5003/swagger/index.html`.
1. Done :)


## Building and packaging

The build and package pipeline for this project is implemented as a set of batch scripts. These scripts can be executed on a development machine without arguments or via a build server (Jenkins, Azure Devops) without the need to lock-in the build process to one specific vendor. This should also make it easier to build older versions of the server, because all of the logic required to build is committed to version control.

To run the scripts, you must have the following installed and accessible in your path:

* 7zip

Here is an overview of our scripts:

| Name                 | Description                                |
| -------------------- | ------------------------------------------ |
| build.bat            | Builds all of the project in release mode  |
|                      | and publishes packages into the folder     |
|                      | `publish` at the root of the solution.     |
| package.bat          | Packages the published files as zip        |
|                      | archives and puts them in `publish`.       |

## Attribution

Some parts of this application are inspired by the work on [Private Tracer](https://gitlab.com/PrivateTracer/server.azure). You can find their license [here](LICENSE/LICENSE.PrivateTracer.org.txt).
