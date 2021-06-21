# COVID-19 Notification App - GGD Portal
This repository contains the front-end portal for GGD workers to validate GGDKey's from CoronaMelder users within the Workflow API backend.

## Introduction

This repository contains the frontend code of the Dutch COVID-19 Notification App GGD portal, technically called `ICC Portal`. 

* The backend repository with Workflow API can be found here https://github.com/minvws/nl-covid19-notification-app-backend.
* The iOS app can be found here: https://github.com/minvws/nl-covid19-notification-app-ios
* The android app can be found here: https://github.com/minvws/nl-covid19-notification-app-android
* The designs that are used as a basis to develop the apps can be found here: https://github.com/minvws/nl-covid19-notification-app-design
* The architecture that underpins the development can be found here: https://github.com/minvws/nl-covid19-notification-app-coordination

## Changelog

The changelog for the UI perspective of this project can be found in [CHANGELOG.md](CHANGELOG.md)

## Development & Contribution process

The core team works on the repository in a private fork (for reasons of compliance with existing processes) and will share its work as often as possible.

If you plan to make non-trivial changes, we recommend to open an issue beforehand where we can discuss your planned changes.
This increases the chance that we might be able to use your contribution (or it avoids doing work if there are reasons why we wouldn't be able to use it).


## Requirements
1. Node JS 12.18.1+  
1. NPM: https://nodejs.org/en/ or Yarn: https://yarnpkg.com/
1. Angular CLI: https://angular.io/guide/setup-local

## Installation

To install all required packages from `package.json`, run the following command:
```bash
npm install
```

## Development server

Run `npm run start` for a dev server. Navigate to [http://localhost:4200/](http://localhost:4200/). The app will automatically reload if you change any of the source files.

## Production build
To make a production build with different environment variables, copy `src/assets/data/appConfig.json` to `src/assets/data/appConfig.prod.json` and adjust the `authHost` properties.

```bash
npm run build --prod
```
