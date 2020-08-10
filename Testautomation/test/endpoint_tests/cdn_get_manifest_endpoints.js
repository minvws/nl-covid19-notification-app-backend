const chai = require("chai");
const expect = chai.expect;
const cdn_get_manifest_controller = require("./endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");
const AdmZip = require("adm-zip");

let apiResponse, content;

describe("Manifest endpoints tests #manifest #endpoints #regression", async function () {
  this.timeout(1000 * 60 * 30);

  before(function () {
    // Do an API request
    return cdn_get_manifest_controller
      .manifest(env.MANIFEST)
      .then(function (response) {
        apiResponse = response.response;
        content = response.content;
        console.log(content);
      });
  });

  it("Manifest response structure validation", function () {
    expect(apiResponse.status, "response status code").to.be.eql(200);
    expect(apiResponse.headers, "response header").to.have.nested.property(
      "content-type",
      "application/zip"
    );

    let total = (
      parseFloat(apiResponse.headers["content-length"]) / 1000
    ).toFixed(3);
    expect(parseInt(total), "Response size below 20KB").to.be.below(20);

    let lastModified = Date.parse(apiResponse.headers["last-modified"]);
    let now = Date.now();
    let maxAge = apiResponse.headers["cache-control"].split("=");
    expect(
      (now - lastModified) / 1000,
      "Response last-modified smaller then max-age"
    ).to.be.below(parseInt(maxAge[1]));
  });

  it("Manifest has all needed property keys", function () {
    expect(content).to.have.nested.property("exposureKeySets");
    expect(content).to.have.nested.property("resourceBundle");
    expect(content).to.have.nested.property("riskCalculationParameters");
    expect(content).to.have.nested.property("appConfig");
  });

  it("Manifest response is no null values", function () {
    expect(content.exposureKeySets,'exposureKeySets').to.be.not.null;
    expect(content.resourceBundle,'resourceBundle').to.be.not.null;
    expect(content.riskCalculationParameters,'riskCalculationParameters').to.be.not.null;
    expect(content.appConfig,'appConfig').to.be.not.null;
  });

  it("Manifest response time is under 200 ms.", function () {
    expect(apiResponse.headers["request-duration"]).to.be.below(200);
  });
});
