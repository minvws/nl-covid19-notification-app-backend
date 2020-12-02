const chai = require("chai");
const expect = chai.expect;
const app_register = require("../../behaviours/app_register_behaviour");
const post_keys = require("../../behaviours/post_keys_behaviour");
const testsSig = require("../../../util/sig_encoding");
const dataprovider = require("../../data/dataprovider");
const formatter = require("../../../util/format_strings");

describe("Postkyes endpoint tests #postkeys #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let app_register_response,
        postkeys_response,
        formated_bucket_id,
        payload,
        version = "v1";

    before(function () {
        return app_register(version).then(function (register) {
            app_register_response = register;
        }).then(function (sig) {
            formated_bucket_id = formatter.format_remove_characters(app_register_response.data.bucketId)
            let map = new Map();
            map.set("BUCKETID", formated_bucket_id);

            payload = dataprovider.get_data("post_keys_payload", "payload", "valid_dynamic_yesterday", map)
            payload = JSON.parse(payload);
            payload = JSON.stringify(payload);

            return testsSig(
                payload,
                formatter.format_remove_characters(app_register_response.data.confirmationKey)
            )
        }).then(function (sig) {
            let map = new Map();
            map.set("BUCKETID", formated_bucket_id);

            return post_keys(
                payload
                ,sig.sig
                ,version).then(function (postkeys) {
                postkeys_response = postkeys;
            });
        })
    });

    after(function (){
        dataprovider.clear_saved();
    })

    it("I should be registered", function () {
        expect(app_register_response.status,"response status code").to.be.eql(200);
    });

    it('I should have send the exposurekeys succesfully', function () {
        expect(postkeys_response.status, 'response status code').to.be.eql(200);
    });

    it('A  postkeys response is empty', function () {
        expect(postkeys_response.data).to.be.empty;
    })

    it('Postkeys response time is under 5 sec.', function () {
        expect(postkeys_response.headers['request-duration'], "response time is below 5 sec.").to.be.below(5000);
    });

});