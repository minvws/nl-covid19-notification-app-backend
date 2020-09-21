const chai = require("chai");
const expect = chai.expect;
const dataprovider = require("../data/dataprovider");
const app_register = require("../behaviours/app_register_behaviour");
const post_keys = require("../behaviours/post_keys_behaviour");
const testsSig = require("../../util/sig_encoding");
const lab_confirm = require("../behaviours/labconfirm_behaviour");
const lab_verify = require("../behaviours/labverify_behaviour");
const manifest = require("../behaviours/manifest_behaviour");
const exposure_key_set = require("../behaviours/exposure_keys_set_behaviour");
const decode_protobuf = require("../../util/protobuff_decoding");
const formatter = require("../../util/format_strings");

describe("Validate push of my exposure key into manifest - #post_key_to_manifest #scenario #regression", function () {
  this.timeout(2000 * 60 * 30);

  // console.log("Scenario: Register > Post keys > Lab Confirm > wait (x min.) > Lab verify > Manifest > EKS")

  let app_register_response,
    postkeys_response,
    lab_confirm_response,
    labConfirmationId,
    pollToken,
    lab_verify_response,
    manifest_response,
    exposure_keyset_response,
    exposure_keyset_decoded,
    formated_bucket_id,
    exposureKeySet,
    exposure_keyset_decoded_set = [];

  before(function () {
    return app_register()
      .then(function (register) {
        app_register_response = register;
        labConfirmationId = register.data.labConfirmationId;
      })
      .then(function () {
        let map = new Map();
        map.set("LABCONFIRMATIONID", formatter.format_remove_dash(labConfirmationId));

        return lab_confirm(dataprovider.get_data("lab_confirm_payload", "payload", "valid_dynamic_yesterday", map)
        ).then(function (confirm) {
          lab_confirm_response = confirm;
          pollToken = confirm.data.pollToken;
        });
      })
      .then(function (sig) {
        formated_bucket_id = formatter.format_remove_characters(app_register_response.data.bucketId);
        let map = new Map();
        map.set("BUCKETID", formated_bucket_id);

        return testsSig.testsSig(
          dataprovider.get_data("post_keys_payload", "payload", "valid_dynamic_yesterday", map),
          formatter.format_remove_characters(app_register_response.data.confirmationKey)
        );
      })
      .then(function (sig) {
        let map = new Map();
        map.set("BUCKETID", formated_bucket_id);

        return post_keys(
          dataprovider.get_data("post_keys_payload", "payload", "valid_dynamic_yesterday", map),
          sig.sig
        ).then(function (postkeys) {
          postkeys_response = postkeys;
        });
      })
      .then(function () {
        return lab_verify(pollToken).then(function (response) {
          lab_verify_response = response;
        });
      })
      .then(function () {
        return manifest().then(function (manifest) {
          manifest_response = manifest;
          exposureKeySet = manifest.content.exposureKeySets;
        });
      })
      .then(async function () {

        function getExposureKeySet(exposureKeySetId){
          return new Promise(function(resolve){
            exposure_key_set(exposureKeySetId).then(function (exposure_keyset) {
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
      });
  });

  after(function (){
    dataprovider.clear_saved();
  })

  it("The exposureKey pushed was in the manifest", function () {
    let exposure_key_send = JSON.parse(
      dataprovider.get_data("post_keys_payload", "payload", "valid_dynamic", new Map())
    ).keys[0].keyData;

    console.log('Number of exposure_keyset_decoded_set: ' + exposure_keyset_decoded_set.length);

    let found = false;
    exposure_keyset_decoded_set.forEach(exposure_keyset_decoded => {
      console.log(exposure_keyset_decoded.exposureKeySet);

      for(key of exposure_keyset_decoded.eks.keys){
          // console.log(`Validating key ${key.keyData} is eql to ${exposure_key_send}`);
          if (key.keyData == exposure_key_send){
            found = true;
          }
      }
    })

    if(found){
      expect(true, `Expected EKS ${exposure_key_send} is found in the manifest`).to.be.eql(true);
    }

    if (!found) {
      expect(true, `Expected EKS ${exposure_key_send} in manifest but not found`).to.be.eql(
          false
      );
    }

  });

});
