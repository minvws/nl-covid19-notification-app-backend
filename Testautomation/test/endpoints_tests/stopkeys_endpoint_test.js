const chai = require("chai");
const expect = chai.expect;
const app_register = require("../behaviours/app_register_behaviour");
const stop_keys = require("../behaviours/post_keys_behaviour");
const testsSig = require("../../util/sig_encoding");
const formater = require("../../util/format_strings");
const dataprovider = require("../data/dataprovider");

describe("Stopkyes endpoints tests #stopkeys #endpoints #regression", function () {
    this.timeout(2000 * 60 * 30);

    let app_register_response, stopkeys_response;

    describe("I can register and send my stopkeys", function () {
    });

    before(function () {
        return app_register().then(function (register) {
            app_register_response = register;
        }).then(function (sig) {
            return testsSig.testsSig(app_register_response.data.bucketId, app_register_response.data.confirmationKey)
        }).then(function (sig) {
            formated_bucket_id = formater(app_register_response.data.bucketId)
            let map = new Map();
            map.set("BUCKETID", formated_bucket_id);

            return testsSig.testsSig(
                dataprovider("post_keys_payload", "payload", "valid_dynamic", map),
                formater(app_register_response.data.confirmationKey)
            )
        })
            .then(function (sig) {
                let map = new Map();
                map.set("BUCKETID", formated_bucket_id);

                return stop_keys(
                    dataprovider("post_keys_payload", "payload", "valid_dynamic", map), sig.sig
                ).then(function (stopkeys) {
                    stopkeys_response = stopkeys;
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
        expect(stopkeys_response.status, 'response status code').to.be.eql(200);
    });

    it('A  postkeys response is empty', function () {
        expect(stopkeys_response.data).to.be.empty;
    })

    it('Postkeys response time is under 5 sec.', function () {
        expect(stopkeys_response.headers['request-duration'], "response time is below 5 sec.").to.be.below(5000);
    });


});