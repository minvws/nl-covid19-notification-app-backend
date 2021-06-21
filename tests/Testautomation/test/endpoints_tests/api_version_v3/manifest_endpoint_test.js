const chai = require("chai");
const expect = chai.expect;
const manifest = require("../../behaviours/manifest_behaviour");

describe("Manifest endpoints tests #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let manifest_response,
        version = "v3";

    before(function () {
        return manifest(version).then(function (manifest) {
            manifest_response = manifest;
        })
    });

    it("I have received the Manifest", function () {
        expect(manifest_response.response.status, "response status code").to.be.eql(200);
    });

    it("Content-type of response is validated", function () {
        expect(manifest_response.response.headers, "response header").to.have.nested.property(
            "content-type",
            "application/zip"
        );
    })

    it("max file size is validated", function () {
        let total = (
            parseFloat(manifest_response.response.headers["content-length"]) / 1000
        ).toFixed(3);
        expect(parseInt(total), "Response size below 20KB").to.be.below(20);
    })

    // validate max-age is not older then 300 sec (5 min.)
    it("Max-Age of manifest data validated, not older then 300 sec.", function () {
        let maxAge = manifest_response.response.headers["cache-control"].split("="); // max age is number of sec.
        maxAge = parseInt(maxAge[1]);

        expect(maxAge - 300,
            `Response max-age (${manifest_response.response.headers["cache-control"]} is not older then ${parseInt(maxAge)} sec. ago`
        ).to.be.least(0);
    });

    it("Manifest has all needed property keys", function () {
        expect(manifest_response.content).to.have.nested.property("exposureKeySets");
        expect(manifest_response.content).to.have.nested.property("riskCalculationParameters");
        expect(manifest_response.content).to.have.nested.property("appConfig");
    });

    it("Manifest response is no null values", function () {
        expect(manifest_response.content.exposureKeySets, 'exposureKeySets').to.be.not.null;
        expect(manifest_response.content.riskCalculationParameters, 'riskCalculationParameters').to.be.not.null;
        expect(manifest_response.content.appConfig, 'appConfig').to.be.not.null;
    });

    it("Manifest response time is under 5 sec.", function () {
        expect(manifest_response.response.headers["request-duration"], "response time is below 5 sec.").to.be.below(5000);
    });

});
