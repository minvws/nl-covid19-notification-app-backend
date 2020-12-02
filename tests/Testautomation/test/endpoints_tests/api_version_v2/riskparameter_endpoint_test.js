const chai = require("chai");
const expect = chai.expect;
const riskparameter = require("../../behaviours/riskparameter_behaviour");
const manifest = require("../../behaviours/manifest_behaviour");

describe("Riskparameter endpoints tests #endpoints #regression", function () {
  this.timeout(2000 * 60 * 30);

  let manifest_response,
      riskparameter_response,
      riskParameterId,
      version = "v2";

  before(function (){
    return manifest(version).then(function (manifest){
      manifest_response = manifest;
      riskParameterId = manifest.content.riskCalculationParameters;

    }).then(function (){
      return riskparameter(riskParameterId, version).then(function (risks){
        riskparameter_response = risks;
      })
    });
  });


  it("I should have received the manifest", function () {
    expect(
        manifest_response.response.status,
      "response status code"
    ).to.be.eql(200);
  });

  it('I should have received the riskparamters', function (){
    expect(riskparameter_response.response.status,'response status code').to.be.eql(200);
    expect(riskparameter_response.response.headers,'response header').to.have.nested.property("content-type","application/zip");

    let total = (parseFloat(riskparameter_response.response.headers["content-length"])/1000).toFixed(3);
    expect(parseInt(total),'Response size below 20KB').to.be.below(20);
  });

  // validate max-age is not older then 1.209.600 sec (14 days)
  it("Max-Age of riskparameters data validated, not older then 1209600 sec. (14 days)", function () {
    let maxAge = riskparameter_response.response.headers["cache-control"].split("="); // max age is number of sec.
    maxAge = parseInt(maxAge[1]);

    expect(1209600 - maxAge,
        `Response max-age ${Math.floor(maxAge/3600/24)} is not older then 1209600 sec. (14 days) ago`
    ).to.be.least(0);
  });


  it('Risk calculation parameter has all needed property keys', function () {
    expect(riskparameter_response.content).to.have.nested.property("minimumRiskScore");
    expect(riskparameter_response.content).to.have.nested.property("attenuationScores");
    expect(riskparameter_response.content).to.have.nested.property("daysSinceLastExposureScores");
    expect(riskparameter_response.content).to.have.nested.property("durationScores");
    expect(riskparameter_response.content).to.have.nested.property("transmissionRiskScores");
    expect(riskparameter_response.content).to.have.nested.property("durationAtAttenuationThresholds");
  });

  it('Risk calculation parameter response is no null values', function (){
    expect(riskparameter_response.content.minimumRiskScore,'minimumRiskScore').to.be.not.null;
    expect(riskparameter_response.content.attenuationScores,'attenuationScores').to.be.not.null;
    expect(riskparameter_response.content.daysSinceLastExposureScores,'daysSinceLastExposureScores').to.be.not.null;
    expect(riskparameter_response.content.durationScores,'durationScores').to.be.not.null;
    expect(riskparameter_response.content.transmissionRiskScores,'transmissionRiskScores').to.be.not.null;
    expect(riskparameter_response.content.durationAtAttenuationThresholds,'durationAtAttenuationThresholds').to.be.not.null;
  });

  it('Risk calculation response time is under 5 sec.', function (){
    expect(riskparameter_response.response.headers['request-duration'],"response time is below 5 sec.").to.be.below(5000);
  });

});