const chai = require("chai");
const expect = chai.expect;
const app_register = require("../../behaviours/app_register_behaviour");

let app_register_response;

describe("Register endpoint test #register #endpoints #scenario #regression", function () {
    this.timeout(2000 * 60 * 30);

    version = "v1";

    before(function () {
        return app_register(version).then(function (register) {
            app_register_response = register;
        })
    });

    it("I should be registered", function () {
        expect(app_register_response.status, "response status code").to.be.eql(200);
        expect(app_register_response.headers, "response header").to.have.nested.property(
            "content-type",
            "application/json; charset=utf-8"
        );
    });

    it("Register has all needed property keys", function () {
        expect(app_register_response.data).to.have.nested.property("labConfirmationId");
        expect(app_register_response.data).to.have.nested.property("bucketId");
        expect(app_register_response.data).to.have.nested.property("confirmationKey");
        expect(app_register_response.data).to.have.nested.property("validity");
    });

    it("Register response is no null values", function () {
        expect(app_register_response.data.labConfirmationId).to.be.not.null;
        expect(app_register_response.data.bucketId).to.be.not.null;
        expect(app_register_response.data.confirmationKey).to.be.not.null;
        expect(app_register_response.data.validity).to.be.not.null;
    });

    it("Register response time is under 5 sec.", function () {
        expect(app_register_response.headers["request-duration"], "response time is below 5 sec.").to.be.below(5000);
    });

});