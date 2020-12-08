const chai = require("chai");
const expect = chai.expect;
const manifest = require("../../behaviours/manifest_behaviour");
const resource_bundle = require("../../behaviours/resourcebundle_behaviour");

describe("ResourceBundle endpoints tests #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let manifest_response,
        resourcebundle_response,
        resoureBundleId,
        version = "v3";

    before(function (){
        return manifest(version).then(function (manifest){
            manifest_response = manifest;
            resoureBundleId = manifest.content.resourceBundle;

        }).then(function (){
            return resource_bundle(resoureBundleId,version).then(function (bundle){
                resourcebundle_response = bundle;
            })
        });
    });

    it("I should have received the manifest", function () {
        expect(
            manifest_response.response.status,
            "response status code"
        ).to.be.eql(200);
    });

    it('A resourcebundle response structure validation', function (){
        //valide response status and type
        expect(resourcebundle_response.response.status,'response status code').to.be.eql(200);
        expect(resourcebundle_response.response.headers,'response header').to.have.nested.property("content-type","application/zip");

        // validate max file size
        let total = (parseFloat(resourcebundle_response.response.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);
    });

    // validate max-age is not older then 1.209.600 sec (14 days)
    it("Max-Age of app config data validated, not older then 1209600 sec. (14 days)", function () {
        let maxAge = resourcebundle_response.response.headers["cache-control"].split("="); // max age is number of sec.
        maxAge = parseInt(maxAge[1]);

        expect(1209600 - maxAge,
            `Response max-age ${Math.floor(maxAge/3600/24)} is not older then 1209600 sec. (14 days) ago`
        ).to.be.least(0);
    });

    it('Resourcebundle has all needed property keys', function () {
        expect(resourcebundle_response.content).to.have.nested.property("resources");
        expect(resourcebundle_response.content).to.have.nested.property("guidance");
    });

    it('Resourcebundle response is no null values', function (){
        expect(resourcebundle_response.content.resources.nl).to.be.not.null;
        expect(resourcebundle_response.content.resources.en).to.be.not.null;
        expect(resourcebundle_response.content.resources.fr).to.be.not.null;
        expect(resourcebundle_response.content.resources.de).to.be.not.null;
        expect(resourcebundle_response.content.resources.tr).to.be.not.null;
        expect(resourcebundle_response.content.resources.pl).to.be.not.null;
        expect(resourcebundle_response.content.resources.ro).to.be.not.null;
        expect(resourcebundle_response.content.resources.ar).to.be.not.null;
        expect(resourcebundle_response.content.resources.fy).to.be.not.null;
        expect(resourcebundle_response.content.resources.es).to.be.not.null;
        expect(resourcebundle_response.content.resources.bg).to.be.not.null;

        expect(resourcebundle_response.content.guidance.quarantineDays).to.be.not.null;
    });

    it('Resourcebundle response time is under 5 sec.', function (){
        expect(resourcebundle_response.response.headers['request-duration'],"response time is below 5 sec.").to.be.below(5000);
    });

});
