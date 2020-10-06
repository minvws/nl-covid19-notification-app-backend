# Corona backend API suite
Marc van 't Veer & Michiel Keij
Polteq.com

## Instructions for setup API CI set

* IDE like IntelliJ: 
* Install node.js (including npm package manager)
* Validate that node and npm are configured [correctly](https://coolestguidesontheplanet.com/installing-node-js-on-macos).
* run from IDE with `run npm install` or from command-line: `npm install` from the `Testautomation` folder
* install mocha via command-line: npm install -g mocha

## Design of the automation suite

The API  suite is JavaScript NodeJS test suite, with the Mocha BDD test framework, Chai assertions, Axios API 
requests and mochawesome reports. There are two main parts in the automation suite:
* endpoints tests (validate the health of each endpoint)
* scenario tests (validate the combination of endpoints under different conditions)

The test suite contains a number of principles to make maintaining and expanding easier.
* The first principle is that we want to split test logic with its implementation. This means that we want to describe what the desired behaviour is and what it's outcome should be.
We don't describe how it should do that. We put this logic in the behaviour objects and the controllers. 
By doing this we lower the amount of work needed when an endpoint is changed without having to change the test itself.

* The second principle is that we want to split test logic with physical data. A dataprovider is added where specific data can be retrieved and altered from. 
The concept is that the required data and it's expected outcome is mentioned in the scenario. That should make it easy to see if the scenario should contain valid data and what type of data is required.
Handling of the physical data is handled in the data object itself.

* The third principle is that every test should have one clear test goal. 
So if the goal is to verify if an exposurekey is in the manifest. That is what we should assert in the it. All other tests obscure why the test was build in the first place.

## Maintaining the test suite

The test suite can be extended by adding new test and by adding assertions to existing tests. A test consists of:
* Describe: name and search terms of the test
* Before: fetching all the required data from the endpoints
* It: assertions (tests) on the response data

### Adding new tests

Before a new test can be added first create a goal of the new test: what needs to be validated in the assertions.
Based on the existing behaviors, endpoints and data providers a test scenario can be created. High over steps to add a new test:
* Create a new branch in Git with the name of the name of the new test
* Create a new js file in the scenario or endpoint tests folder
* Add all the required dependencies like chai and expect
* Fill the description with a logical name and search term (using a # before the search term)
* Fill the before with the intended behavior
    * Use behaviors and controllers and save responses
    * Chain the requests into a flow and wait for the responses
* Create the assertions with expect from chai

### Test data
Test data can be added to the following [folder](Testautomation/test/data). With the dataprovider function this can be used
in a test. Test data can be of different type, like postkeys or register padding, and of variants of the same type, like postkey
of yesterday of two weeks ago. Within the different types to create dynamic testdata, like timestamps.

### API versions
There are multiple API versions, at the moment of writing v1 and v2. This version can be set at different points. The lowest point
is at runtime, with --version=v1 option on the command-line. At a higher leverl the version can be set within a tests as an variable.
Then this test will be run for this version. The last option to use version is in scenario's where multiple endpoints with different version
can be used with the variable currentVersion (v1) and nextVersion (v2).

## Running the test suite

### Pre-reqs

In order to run the test suite, you need to be running a linux-compatible terminal. On Windows this can be achieved by installing WSL (https://docs.microsoft.com/en-us/windows/wsl/install-win10) or by using a terminal such as CMDER (https://cmder.net).

You need the following commmands available in your path:
- `cat`
- `openssl`
- `base64`
- `sed`

You also need a bearer token for the GGD Portal. You must pass this as a CLI parameter to mocha. The token looks like this:

`Bearer fgGgrdsd573fghk643`

You need to replace `[bearer-token]` with the `fgGgrdsd573fghk643` part of the token.

### Run the tests

* Open terminal, navigate to root folder of repo then go to the folder: `Testautomation`
* Each collection is in a group that starts with a # sign. The groups are:
    * endpoints => only runs endpoint tests
    * scenario's => only runs scenario's tests
    * regression => runs the whole API test suite
* To run a collection the following command can be used
    * `mocha --recursive --grep '#endpoints' --environment=TST --version=v1 --reporter mocha-junit-reporter --reporter-options mochaFile=./reports/junit/file.xml --token=[bearer-token]`
    * `mocha --recursive --grep '#endpoints' --environment=TST --version=v1 --reporter mochawesome --reporter-options reportDir=./reports/mochawesome,reportFilename=mochawesome --token=[bearer-token]`
        * `[bearer-token]`: this is the token sans the prefix `Bearer `, as described above.
        * `recursive`: fetches all test on lower directories
        * `grep`: search term in header of every test
        * `#endpoints`: search term by grep
        
    * Optional parameters:
        * `environment`: to switch between an environment like TST for test, ACC for acceptance and PROD for production
        * `version`: to switch between a version of the api
        * `reporter`: which report is created when running the test, besides junit, mochawesome (HTML/jSON) can also used
        * `reporter-options`: location where the report is saved and the name of the report

