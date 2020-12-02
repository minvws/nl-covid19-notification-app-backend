const chai = require("chai");
const expect = chai.expect;
const moment = require("moment");
const manifest = require("../../behaviours/manifest_behaviour");
const exposure_key_set = require("../../behaviours/exposure_keys_set_behaviour");
const decode_protobuf = require("../../../util/protobuff_decoding");

describe("Manifest endpoints tests #multiple_exposure_keys #scenario #regression", function () {
    this.timeout(4000 * 60 * 30);

    let manifest_response,
        exposureKeySet,
        exposure_keyset_response,
        exposure_keyset_decoded,
        currentVersion = "v1",
        nextVersion = "v2",
        exposure_keyset_decoded_set = [];

    before(function () {
        return manifest(nextVersion).then(function (manifest) {
            manifest_response = manifest;
            exposureKeySet = manifest.content.exposureKeySets;
        }).then(async function () {

            function getExposureKeySet(exposureKeySetId){
                return new Promise(function(resolve){
                    exposure_key_set(exposureKeySetId, nextVersion).then(function (exposure_keyset) {
                        exposure_keyset_response = exposure_keyset;
                        return decode_protobuf(exposure_keyset_response)
                            .then(function (EKS) {
                                return resolve(exposure_keyset_decoded = EKS)
                            });
                    })
                });
            }

            for(i = 0; i< exposureKeySet.length; i++){
                let eks = await getExposureKeySet(exposureKeySet[i])
                let TemporaryExposureKey = eks.keys
                // decode keydata into readable text
                TemporaryExposureKey.forEach(key => {
                    key.keyData = key.keyData.toString("base64");
                })
                let obj = {"exposureKeySet" : exposureKeySet[i],
                            "eks":eks
                }
                exposure_keyset_decoded_set.push(obj);
            }
        })
    });

    it("Validate max number of keys in keyset is below 98", function(){
        console.log('Number of exposure_keyset_decoded_set: ' + exposure_keyset_decoded_set.length);
        expect(exposure_keyset_decoded_set.length).to.be.most(98);
    })

    // validate max-age is not older then 1.209.600 sec (14 days)
    it("Exposure keyset not older then 14 days", function () {
        // end timestamp is not older then 14 days run from yesterday
        let max_days_old = moment().add(-15,'days').unix();

        exposure_keyset_decoded_set.forEach(exposure_keyset_decoded => {
            // console.log('end date key: ' + moment.unix(exposure_keyset_decoded.eks.endTimestamp.low).format('dddd, MMMM Do, YYYY h:mm:ss A'));
            expect(exposure_keyset_decoded.eks.endTimestamp.low - max_days_old,"key not older then 14 days").to.be.least(0);
        })
    });


    it("Exposure Key sets has all needed property keys", function () {
        exposure_keyset_decoded_set.forEach(exposure_keyset_decoded => {
            console.log(exposure_keyset_decoded.exposureKeySet);
            expect(exposure_keyset_decoded.eks).to.have.nested.property("startTimestamp");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("endTimestamp");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("region");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("batchNum");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("batchSize");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("signatureInfos");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("keys");
            expect(exposure_keyset_decoded.eks).to.have.nested.property("revisedKeys");
            expect(exposure_keyset_decoded.eks.signatureInfos[0]).to.have.nested.property("verificationKeyVersion");
            expect(exposure_keyset_decoded.eks.signatureInfos[0]).to.have.nested.property("verificationKeyId");
            expect(exposure_keyset_decoded.eks.signatureInfos[0]).to.have.nested.property("signatureAlgorithm");

            console.log('Number of temp exposure keys: ' + exposure_keyset_decoded.eks.keys.length)
            exposure_keyset_decoded.eks.keys.forEach(key => {
                expect(key).to.have.nested.property("keyData").that.is.not.null;
                expect(key).to.have.nested.property("transmissionRiskLevel").that.is.not.null;
                expect(key).to.have.nested.property("transmissionRiskLevel").to.be.oneOf([1,2,3]);
                expect(key).to.have.nested.property("rollingStartIntervalNumber").that.is.not.null;
                expect(key).to.have.nested.property("rollingPeriod").that.is.not.null;
                expect(key).to.have.nested.property("rollingPeriod").to.be.within(1,144);
            })
        });

    });

});