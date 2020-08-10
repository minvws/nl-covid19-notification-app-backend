const chai = require('chai');
const expect = chai.expect;
const cdn_get_riskparameter_controller = require("./endpoint_controllers/cdn_get_riskparameters_controller");
const cdn_get_manifest_controller = require("./endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");
const AdmZip = require('adm-zip');

let apiResponse, content, etag, riskParameterId;

describe('Riskcalcalculationparameter endpoint tests #riskparameter #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        // Do an API request
        return cdn_get_manifest_controller.manifest(env.MANIFEST).then(function (response){
            apiResponse = response.response;
            content = response.content;

            // [todo] update appconfig if etag is changed in manifest
            riskParameterId = content.riskCalculationParameters;

        }).then(function (){
            // Do an API request
            return cdn_get_riskparameter_controller.riskparameters(env.RISKPARAMETERS,riskParameterId).then(function (response){
                apiResponse = response;
                let data = apiResponse.data;
                let zip = new AdmZip(apiResponse.data);
                let zipEntries = zip.getEntries();
                content = JSON.parse(zip.readAsText(zipEntries[0]).toString('utf8'));
                // console.log(content);
            })
        });
    });

    it('A risk calculation parameter response structure validation', function (){
        expect(apiResponse.status,'response status code').to.be.eql(200);
        expect(apiResponse.headers,'response header').to.have.nested.property("content-type","application/zip");

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

        let lastModified = (Date.parse(apiResponse.headers["last-modified"]));
        let now = Date.now();
        let maxAge = apiResponse.headers["cache-control"].split("=");
        expect((now-lastModified)/1000,'Response last-modified smaller then max-age').to.be.below(parseInt(maxAge[1]));
    });

    it('Risk calculation parameter has all needed property keys', function () {
        expect(content).to.have.nested.property("minimumRiskScore");
        expect(content).to.have.nested.property("attenuationScores");
        expect(content).to.have.nested.property("daysSinceLastExposureScores");
        expect(content).to.have.nested.property("durationScores");
        expect(content).to.have.nested.property("transmissionRiskScores");
        expect(content).to.have.nested.property("durationAtAttenuationThresholds");
    });

    it('Risk calculation parameter response is no null values', function (){
        expect(content.minimumRiskScore,'minimumRiskScore').to.be.not.null;
        expect(content.attenuationScores,'attenuationScores').to.be.not.null;
        expect(content.daysSinceLastExposureScores,'daysSinceLastExposureScores').to.be.not.null;
        expect(content.durationScores,'durationScores').to.be.not.null;
        expect(content.transmissionRiskScores,'transmissionRiskScores').to.be.not.null;
        expect(content.durationAtAttenuationThresholds,'durationAtAttenuationThresholds').to.be.not.null;
    });

    it('Risk calculation response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });
});