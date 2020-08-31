const chai = require("chai");
const expect = chai.expect;
const app_register = require("../behaviours/app_register_behaviour");
const post_keys = require("../behaviours/post_keys_behaviour");
const testsSig = require("../../util/sig_encoding");
const dataprovider = require("../data/dataprovider");
const formater = require("../../util/format_strings").formater;

describe("Postkyes endpoints tests #postkeys #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let app_register_response, postkeys_response, formated_bucket_id;

    before(function () {
        return app_register().then(function (register) {
            app_register_response = register;
        }).then(function (sig) {
            formated_bucket_id = formater(app_register_response.data.bucketId)
            let map = new Map();
            map.set("BUCKETID", formated_bucket_id);

            return testsSig.testsSig(
                dataprovider("post_keys_payload", "payload", "valid_dynamic", map),
                formater(app_register_response.data.confirmationKey)
            )
        }).then(function (sig) {
            let map = new Map();
            map.set("BUCKETID", formated_bucket_id);

            return post_keys(
                dataprovider("post_keys_payload", "payload", "valid_dynamic", map)
                ,sig.sig).then(function (postkeys) {
                postkeys_response = postkeys;
            });
        })
    });

    it("I should be registered", function () {
        expect(
            app_register_response.status,
            "response status code"
        ).to.be.eql(200);
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