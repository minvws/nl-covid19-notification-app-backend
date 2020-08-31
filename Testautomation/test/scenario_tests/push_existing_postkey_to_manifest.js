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
const formater = require("../../util/format_strings").formater;
const formater_labconfirm = require("../../util/format_strings").format_lab_confirm_id;

describe("Validate push of my exposure key into manifest - #expose_keys #scenario #regression", function () {
  this.timeout(2000 * 60 * 30);

  // console.log("Scenario: Register > Post keys > Lab Confirm > wait (x min.) > Lab verify > Manifest > EKS")

  let app_register_response,
    postkeys_response,
    lab_confirm_response,
    labConfirmationId,
    pollToken,
    lab_verify_response,
    manifest_response,
    exposureKeySetId,
    exposure_keyset_response,
    exposure_keyset_decoded,
    formated_bucket_id;

  before(function () {
    return app_register()
      .then(function (register) {
        app_register_response = register;
        labConfirmationId = register.data.labConfirmationId;
      })
      .then(function (sig) {
        formated_bucket_id = formater(app_register_response.data.bucketId);
        let map = new Map();
        map.set("BUCKETID", formated_bucket_id);

        return testsSig.testsSig(
          dataprovider("post_keys_payload", "payload", "valid_dynamic", map),
          formater(app_register_response.data.confirmationKey)
        );
      })
      .then(function (sig) {
        let map = new Map();
        map.set("BUCKETID", formated_bucket_id);

        return post_keys(
          dataprovider("post_keys_payload", "payload", "valid_dynamic", map),
          sig.sig
        ).then(function (postkeys) {
          postkeys_response = postkeys;
        });
      })
      .then(function () {
        let map = new Map();
        map.set("LABCONFIRMATIONID", formater_labconfirm(labConfirmationId));

        return lab_confirm(dataprovider("lab_confirm_payload", "payload", "valid_dynamic", map)
        ).then(function (confirm) {
          lab_confirm_response = confirm;
          pollToken = confirm.data.pollToken;
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
          exposureKeySetId = manifest.content.exposureKeySets[0];
        });
      })
      .then(async function () {
        return exposure_key_set(exposureKeySetId).then(function (
          exposure_keyset
        ) {
          exposure_keyset_response = exposure_keyset;
          return decode_protobuf(exposure_keyset_response).then(function (EKS) {
            exposure_keyset_decoded = EKS;
          });
        });
      });
  });

  it("The exposureKey pushed was in the manifest", function () {
    let exposure_key_send = JSON.parse(
      dataprovider("post_keys_payload", "payload", "valid_dynamic", new Map())
    ).keys[0].keyData;

    let found = false;
    for (key of exposure_keyset_decoded.keys) {
      if (key.keyData.toString("base64") == exposure_key_send) {
        expect(key.keyData.toString("base64")).to.be.eql(exposure_key_send);
        found = true;
        break;
      }
    }
    if (!found) {
      expect(true, "Expected EKS in manifest keys but not found").to.be.eql(
        false
      );
    }
  });
});
