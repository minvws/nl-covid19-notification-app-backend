const chai = require("chai");
const expect = chai.expect;
const riskparameter = require("../behaviours/riskparameter_behaviour");
const manifest = require("../behaviours/manifest_behaviour");

describe("Riskparameter endpoints tests #riskparameter #endpoints #regression", function () {
  this.timeout(2000 * 60 * 30);

  let manifest_response, riskparameter_response, riskParameterId;

  before(function (){
    return manifest().then(function (manifest){
      manifest_response = manifest;
      riskParameterId = manifest.content.riskCalculationParameters;

    }).then(function (){
      return riskparameter(riskParameterId).then(function (risks){
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

    let lastModified = (Date.parse(riskparameter_response.response.headers["last-modified"]));
    let now = Date.now();
    let maxAge = riskparameter_response.response.headers["cache-control"].split("=");
    expect((now-lastModified)/1000,'Response last-modified smaller then max-age').to.be.below(parseInt(maxAge[1]));
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