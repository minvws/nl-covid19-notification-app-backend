const chai = require("chai");
const expect = chai.expect;
const manifest = require("../behaviours/manifest_behaviour");

describe("Manifest endpoints tests #manifest #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let manifest_response;

    before(function () {
        return manifest().then(function (manifest) {
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

    // validate lastmodified is not
    it("Max-Age of exposureKey data validated", function () {
        let lastModified = Date.parse(manifest_response.response.headers["last-modified"]);
        let now = Date.now();
        let maxAge = manifest_response.response.headers["cache-control"].split("="); // max age is number of sec.

        let difference = (parseInt(now) - parseInt(lastModified)) / 1000
        // console.log('difference in sec: ' + difference.toFixed(2));
        // console.log('maxAge: ' + maxAge[1]);

        expect(
            (now - lastModified) / 1000,
            `Response last-modified (${manifest_response.response.headers["last-modified"]} is not older then ${parseInt(maxAge[1])} sec. ago`
        ).to.be.below(parseInt(maxAge[1]));
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