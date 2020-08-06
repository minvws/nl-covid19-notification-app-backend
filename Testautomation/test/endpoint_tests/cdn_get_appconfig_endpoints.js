const chai = require('chai');
const expect = chai.expect;
const cdn_get_appconfig_controller = require("./endpoint_controllers/cdn_get_appconfig_controller");
const cdn_get_manifest_controller = require("./endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");
const AdmZip = require('adm-zip');

let apiResponse, content, etag, appConfigId;

describe('Appconfig endpoints tests #appconfig #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        // Do an API request
        return cdn_get_manifest_controller.manifest(env.MANIFEST).then(function (response){
            apiResponse = response.response;
            content = response.content;

            // [todo] update appconfig if etag is changed in manifest
            appConfigId = content.appConfig;

        }).then(function (){
            // Do an API request
            return cdn_get_appconfig_controller.appconfig(env.APPCONFIG,appConfigId).then(function (response){
                apiResponse = response;
                let data = apiResponse.data;
                let zip = new AdmZip(apiResponse.data);
                let zipEntries = zip.getEntries();
                content = JSON.parse(zip.readAsText(zipEntries[0]).toString('utf8'));
                console.log(content);
            })
        });
    });

    it('A appconfig response structure validation', function (){
        expect(apiResponse.status,'response status code').to.be.eql(200);
        expect(apiResponse.headers,'response header').to.have.nested.property("content-type","application/zip");

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

        let lastModified = (Date.parse(apiResponse.headers["last-modified"]));
        let now = Date.now();
        let maxAge = apiResponse.headers["cache-control"].split("=");
        expect((now-lastModified)/1000,'Response last-modified smaller then max-age').to.be.below(parseInt(maxAge[1]));
    });

    it('Appconfig has all needed property keys', function () {
        expect(content).to.have.nested.property("androidMinimumVersion");
        expect(content).to.have.nested.property("androidMinimumKillVersion");
        expect(content).to.have.nested.property("iOSMinimumVersion");
        expect(content).to.have.nested.property("iOSMinimumKillVersion");
        expect(content).to.have.nested.property("iOSAppStoreURL");        
        expect(content).to.have.nested.property("manifestFrequency");
        expect(content).to.have.nested.property("decoyProbability");
        expect(content).to.have.nested.property("requestMinimumSize");
        expect(content).to.have.nested.property("requestMaximumSize");
        expect(content).to.have.nested.property("repeatedUploadDelay");
    });

    it('Appconfig response is no null values', function (){
        expect(content.androidMinimumVersion,'androidMinimumVersion').to.be.not.null;
        expect(content.androidMinimumKillVersion,'androidMinimumKillVersion').to.be.not.null;
        expect(content.iOSMinimumVersion,'iOSMinimumVersion').to.be.not.null;
        expect(content.iOSMinimumKillVersion,'iOSMinimumKillVersion').to.be.not.null;
        expect(content.iOSAppStoreURL,'iOSAppStoreURL').to.be.not.null;
        expect(content.manifestFrequency,'manifestFrequency').to.be.not.null;
        expect(content.decoyProbability,'decoyProbability').to.be.not.null;
        expect(content.requestMinimumSize,'requestMinimumSize').to.be.not.null;
        expect(content.requestMaximumSize,'requestMaximumSize').to.be.not.null;
        expect(content.repeatedUploadDelay,'repeatedUploadDelay').to.be.not.null;
    });

    it('Appconfig response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });

});