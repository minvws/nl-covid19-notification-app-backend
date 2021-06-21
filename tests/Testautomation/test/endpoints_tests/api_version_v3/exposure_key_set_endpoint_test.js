const chai = require("chai");
const expect = chai.expect;
const manifest = require("../../behaviours/manifest_behaviour");
const exposure_key_set = require("../../behaviours/exposure_keys_set_behaviour");
const decode_protobuf = require("../../../util/protobuff_decoding");

describe("Exposure key set endpoints tests #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let manifest_response,
        exposureKeySetId,
        exposure_keyset_response,
        exposure_keyset_decoded,
        version = "v3";

    before(function () {
        return manifest(version).then(function (manifest) {
            manifest_response = manifest;
            exposureKeySetId = manifest.content.exposureKeySets[0];
        }).then(async function () {
            return exposure_key_set(exposureKeySetId,version).then(function (exposure_keyset) {
                exposure_keyset_response = exposure_keyset;
                return decode_protobuf(exposure_keyset_response)
                    .then(function (EKS) {
                        exposure_keyset_decoded = EKS;
                    });
            })
        })
    });

    it("I have received the Manifest", function () {
        expect(manifest_response.response.status, "response status code").to.be.eql(200);
    });

    it("I have received the EKS", function () {
        expect(exposure_keyset_response.status, "response status code").to.be.eql(200);
        expect(exposure_keyset_response.headers, "response header").to.have.nested.property(
            "content-type",
            "application/zip"
        );
    })

    // validate max-age is not older then 1227600 sec (14 days + 5hrs)
    it("Max-Age of exposureKey data validated, not older then 1227600 sec. (14 days + 5 hrs)", function () {
        let maxAge = exposure_keyset_response.headers["cache-control"].split("="); // max age is number of sec.
        maxAge = parseInt(maxAge[1]);
        let maxPossibleAge = (3600*24*14) + (3600*5);
        expect(maxPossibleAge - maxAge,
            `Response max-age ${Math.floor(maxAge)} is not older then ${maxPossibleAge} sec. (14 days + 5hrs) ago`
        ).to.be.least(0);
    });

    it("Exposure Key sets response time is under 5 sec.", function () {
        expect(exposure_keyset_response.headers["request-duration"], "response time is below 5 sec.").to.be.below(5000);
    });

    it("Exposure Key sets has all needed property keys", function () {
        let TemporaryExposureKey = exposure_keyset_decoded.keys
        TemporaryExposureKey.forEach(key => {
            key.keyData = key.keyData.toString("base64");
        })
        // console.log(exposure_keyset_decoded.keys);

        expect(exposure_keyset_decoded).to.have.nested.property("startTimestamp");
        expect(exposure_keyset_decoded).to.have.nested.property("startTimestamp");
        expect(exposure_keyset_decoded).to.have.nested.property("endTimestamp");
        expect(exposure_keyset_decoded).to.have.nested.property("region");
        expect(exposure_keyset_decoded).to.have.nested.property("batchNum");
        expect(exposure_keyset_decoded).to.have.nested.property("batchSize");
        expect(exposure_keyset_decoded).to.have.nested.property("signatureInfos");
        expect(exposure_keyset_decoded).to.have.nested.property("keys");
        expect(exposure_keyset_decoded).to.have.nested.property("revisedKeys");
        expect(exposure_keyset_decoded.signatureInfos[0]).to.have.nested.property("verificationKeyVersion");
        expect(exposure_keyset_decoded.signatureInfos[0]).to.have.nested.property("verificationKeyId", "204");
        expect(exposure_keyset_decoded.signatureInfos[0]).to.have.nested.property("signatureAlgorithm", "1.2.840.10045.4.3.2");

        exposure_keyset_decoded.keys.forEach(key => {
            expect(key).to.have.nested.property("keyData").that.is.not.null;
            expect(key).to.have.nested.property("transmissionRiskLevel").that.is.not.null;
            expect(key).to.have.nested.property("rollingStartIntervalNumber").that.is.not.null;
            expect(key).to.have.nested.property("rollingPeriod").that.is.not.null;
        })

    });

});
