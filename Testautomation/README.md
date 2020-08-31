# Corona backend API suite

## Instructions for setup API CI set

* IDE like IntelliJ: 
* Install node.js (including npm package manager)
* Validate that node and npm are configured correctly: https://coolestguidesontheplanet.com/installing-node-js-on-macos/
* run from IDE with "Run npm install" or from command-line: npm install from the apitest folder
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
Handling of the fysical data is handled in the data object itself.

* The third principle is that every test should have one clear test goal. 
So if the goal is to verify if an exposurekey is in the manifest. That is what we should assert in the it. All other tests obscure why the test was build in the first place.

## Maintaining the test suite
The test suite can be extend by adding new test and by adding assertions to existing tests. A test consists of:
* Describe: name and search terms of the test
* Before: fetching all the required data from the endpoints
* It: assertions (tests) on the response data

### Adding new tests
Before a new test can be added first create a goal of the new test: what needs to be validated in the assertions.
Based on the existing behaviors, endpoints and data providers a test scenario can be created. High over steps to add a new test:
* Create a new branch in Git with the name of the name of the new test
* Create a new js file in the scenario or endpoint tests folder
* Add all the required dependencies like chai and expect
* Fill the describe with a logical name and search term (using a # before the search term)
* Fill the before with the intended behavior
    * Use behaviors and controllers and save responses
    * Chain the requests into a flow and wait for the responses
* Create the assertions with expect from chai

## Running the test suite
* Open terminal and navigate to root folder of repo: cd MSS-nl-covid19-notification-app-backend/Testautomation/test
* Each collection is in a group that starts with a # sign. The groups are:
    * endpoints => only runs endpoint tests
    * scenario's => only runs scenario's tests
    * regression => runs the whole API test suite
* To run a collection the following command can be used
    * mocha --recursive --grep '#endpoints' --environment=TST --reporter mocha-junit-reporter --reporter-options mochaFile=./reports/junit/file.xml
    * mocha --recursive --grep '#endpoints' --environment=TST --reporter mochawesome --reporter-options reportDir=./reports/mochawesome,reportFilename=mochawesome
        ** recursive: fetches all test on lower directories
        ** grep: search term in header of every test
        ** #endpoints: search term by grep
        ** environment: to switch between an environment like TST for test, ACC for acceptance and PROD for production
        ** reporter: which report is created when running the test, besides junit, mochawesome (HTML/jSON) can also used
        ** reporter-options: location where the report is saved and the name of the report

