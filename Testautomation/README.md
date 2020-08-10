# Corona backend API suite

## Instructions for setup API CI set

* IDE like IntelliJ: 
* Install node.js (including npm package manager)
* Validate that node and npm are configured correctly: https://coolestguidesontheplanet.com/installing-node-js-on-macos/
* run from IDE with "Run npm install" or from command-line: npm install from the apitest folder
* install mocha via command-line: npm install -g mocha

## Extra info
The API  suite is JavaScript NodeJS test suite, with the Mocha BDD test framework, Chai assertions, Axios API 
requests and mochawesome reports. There are two main parts in the automation suite with each a separate report:
* endpoints tests
* scenario tests

## setup the environment settings
*
npm install

## Maintaining the test suite
* 
All tests are in test/endpoint_tests

### Add a new tests
* 

### Update a tests
* 

## Running the test suite
* Open terminal and navigate to root folder of repo: cd MSS-nl-covid19-notification-app-backend/Testautomation/test/endpoint_tests
* Each collection is in a group that starts with a # sign. The groups are:
    * [**#register**](test/endpoint_tests/app_post_register_endpoints.js) => only runs the register
    * [**#endpoints**](test/endpoint_tests/app_post_register_endpoints.js) => only runs endpoint tests
    * [**#scenario**](test/scenario_tests/test/) => only runs scenario's tests
    * [**#regression**] => runs the whole API test suite
* There are three environments, which  can be found in the [**util/env/conf**](util/env_config.js) file:
    * Test
    * Acceptance
    * Production
* You can switch between the two environments via the command line option:
    * --environment
* A test is executed from the command line with
    * mocha --grep "#endpoints" --environment=TST
    * for example: mocha app_post_postkeys_endpoints.js --enviroment=ACC --version=v1 
* If a different version this can be added as a command line option:
    * mocha --grep "#endpoints" --environment=TST --version=v1
* The output is in the reports folder in CLI and HTML format. The reports are recycled and updated on each run.
* If a test fails then analysis via report and rerun a specific test (endpoint of scenario) again
