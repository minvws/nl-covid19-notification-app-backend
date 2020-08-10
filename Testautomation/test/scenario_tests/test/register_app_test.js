const chai = require("chai");
const expect = chai.expect;
const app_register = require("../behaviours/app_register_behaviour");

// const app_register_response;

describe("Default-name-for-this-test #some-tags", async function () {
    describe("I register the app"), async function () {
        // app_register_response = app_register("valid", "scenA");
    };
});

it("I should be registered", function () {
  expect(app_register_response.status, "response status code").to.be.eql(200);
});
