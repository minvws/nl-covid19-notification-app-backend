const chai = require("chai");
const expect = chai.expect;
const app_config = require("../behaviours/appconfig_behaviour");
const manifest = require("../behaviours/manifest_behaviour");

describe("Appconfig endpoints tests #appconfig #endpoints #regression", function () {
  this.timeout(2000 * 60 * 30);

  let manifest_response, appconfig_response, appConfigId;

  before(function (){
    return manifest().then(function (manifest){
      manifest_response = manifest;
      appConfigId = manifest.content.appConfig;

    }).then(function (){
      return app_config(appConfigId).then(function (config){
        appconfig_response = config;
      })
    });
  });


  it("I should have received the manifest", function () {
    expect(
        manifest_response.response.status,
      "response status code"
    ).to.be.eql(200);
  });

  it('A appconfig response structure validation', function (){
    //valide response status and type
    expect(appconfig_response.response.status,'response status code').to.be.eql(200);
    expect(appconfig_response.response.headers,'response header').to.have.nested.property("content-type","application/zip");

    // validate max file size
    let total = (parseFloat(appconfig_response.response.headers["content-length"])/1000).toFixed(3);
    expect(parseInt(total),'Response size below 20KB').to.be.below(20);
  });

  // validate lastmodified is not
  it("Max-Age of exposureKey data validated", function (){
    let lastModified = Date.parse(appconfig_response.response.headers["last-modified"]);
    let now = Date.now();
    let maxAge = appconfig_response.response.headers["cache-control"].split("=");
    expect(
        (now - lastModified) / 1000,
        `Response last-modified (${appconfig_response.response.headers["last-modified"]} is not older then ${maxAge[1]/3600} hours ago`
    ).to.be.below(parseInt(maxAge[1]));
  });

  it('Appconfig has all needed property keys', function () {
    expect(appconfig_response.content).to.have.nested.property("androidMinimumVersion");
    expect(appconfig_response.content).to.have.nested.property("iOSMinimumVersion");
    expect(appconfig_response.content).to.have.nested.property("iOSMinimumVersionMessage");
    expect(appconfig_response.content).to.have.nested.property("iOSAppStoreURL");
    expect(appconfig_response.content).to.have.nested.property("manifestFrequency");
    expect(appconfig_response.content).to.have.nested.property("decoyProbability");
    expect(appconfig_response.content).to.have.nested.property("requestMinimumSize");
    expect(appconfig_response.content).to.have.nested.property("requestMaximumSize");
    expect(appconfig_response.content).to.have.nested.property("repeatedUploadDelay");
  });

  it('Appconfig response is no null values', function (){
    expect(appconfig_response.content.androidMinimumVersion,'androidMinimumVersion').to.be.not.null;
    expect(appconfig_response.content.iOSMinimumVersion,'iOSMinimumVersion').to.be.not.null;
    expect(appconfig_response.content.iOSMinimumKillVersion,'iOSMinimumVersionMessage').to.be.not.null;
    expect(appconfig_response.content.iOSAppStoreURL,'iOSAppStoreURL').to.be.not.null;
    expect(appconfig_response.content.manifestFrequency,'manifestFrequency').to.be.not.null;
    expect(appconfig_response.content.decoyProbability,'decoyProbability').to.be.not.null;
    expect(appconfig_response.content.requestMinimumSize,'requestMinimumSize').to.be.not.null;
    expect(appconfig_response.content.requestMaximumSize,'requestMaximumSize').to.be.not.null;
    expect(appconfig_response.content.repeatedUploadDelay,'repeatedUploadDelay').to.be.not.null;
  });

  it('Appconfig response time is under 5 sec.', function (){
    expect(appconfig_response.response.headers['request-duration'],"response time is below 5 sec.").to.be.below(5000);
  });

});