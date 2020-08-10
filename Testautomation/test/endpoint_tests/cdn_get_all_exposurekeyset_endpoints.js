const chai = require("chai");
const expect = chai.expect;
const cdn_get_exposurekeyset_controller = require("./endpoint_controllers/cdn_get_exposurekeyset_controller");
const cdn_get_manifest_controller = require("./endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");
const decode_protobuff = require("../../util/protobuff_decoding");

let apiResponse, content,exposureKeySets,contentArray = [];

describe("Exposure Key sets endpoints tests #exposurekeyset #endpoints #regression", async function () {
  this.timeout(1000 * 60 * 30);

    before(function () {

        function exposureKeys() {
            return new Promise(function (resolve) {
                cdn_get_manifest_controller
                    .manifest(env.MANIFEST)
                    .then(function (response) {
                        apiResponse = response.response;
                        content = response.content;
                        exposureKeySetId = content.exposureKeySets[2];
                        exposureKeySets = content.exposureKeySets;
                        return resolve(exposureKeySets);
                    });
            })
        }

        return exposureKeys().then(function (response) {
            // content = response
            response.forEach(exposureKeySetId => {
                cdn_get_exposurekeyset_controller.exposurekeyset(env.EXPOSUREKEYSET, exposureKeySetId).then(function (response) {
                    // data decode and parse to json
                    try{
                        decode_protobuff.decode_proto_buff(response, function (resp) {
                            // exposureKeysFound = resp.keys;
                            return contentArray.push(JSON.parse(JSON.stringify(resp)));
                        })
                    }
                    catch (err){
                        console.log(err);
                    }

                })
            });
        }).then(console.log('test' + contentArray));
    });

        // cdn_get_exposurekeyset_controller
        //     .exposurekeyset(env.EXPOSUREKEYSET, exposureKeySetId)
        //     .then(function (response) {
        //         // data decode and parse to json
        //         decode_protobuff.decode_proto_buff(response, function (resp) {
        //             // exposureKeysFound = resp.keys;
        //             return resolve(content = JSON.parse(JSON.stringify(resp)));
        //         });
        //     });
    // });


  // add loop through the exposurekeyset array from the manifest end validate every content file


    it("A Exposure Key set response structure validation", function () {
      expect(apiResponse.status, "response status code").to.be.eql(200);
      expect(apiResponse.headers, "response header").to.have.nested.property(
        "content-type",
        "application/zip"
      );
    });

    // it("Max-Age of exposureKey data validated", function (){
    //   let lastModified = Date.parse(apiResponse.headers["last-modified"]);
    //   let now = Date.now();
    //   let maxAge = apiResponse.headers["cache-control"].split("=");
    //   expect(
    //       (now - lastModified) / 1000,
    //       `Response last-modified (${apiResponse.headers["last-modified"]} is not older then ${maxAge[1]/3600} hours ago`
    //   ).to.be.below(parseInt(maxAge[1]));
    // });
    //
    // it("Exposure Key sets response time is under 200 ms.", function () {
    //     expect(apiResponse.headers["request-duration"]).to.be.below(200);
    // });
    //
    // it("Exposure Key sets has all needed property keys", function () {
    //
    //     expect(content).to.have.nested.property("endTimestamp");
    //     expect(content).to.have.nested.property("region");
    //     expect(content).to.have.nested.property("batchNum");
    //     expect(content).to.have.nested.property("batchSize");
    //     expect(content).to.have.nested.property("signatureInfos");
    //     expect(content).to.have.nested.property("keys");
    //     expect(content.signatureInfos[0]).to.have.nested.property("verificationKeyVersion","v1");
    //     expect(content.signatureInfos[0]).to.have.nested.property("verificationKeyId","204");
    //     expect(content.signatureInfos[0]).to.have.nested.property("signatureAlgorithm","1.2.840.10045.4.3.2");
    //     expect(content.keys[0]).to.have.nested.property("keyData").that.is.not.null;
    //     expect(content.keys[0]).to.have.nested.property("transmissionRiskLevel").that.is.not.null;
    //     expect(content.keys[0]).to.have.nested.property("rollingStartIntervalNumber").that.is.not.null;
    //     expect(content.keys[0]).to.have.nested.property("rollingPeriod").that.is.not.null;
    // });

});
