const chai = require('chai');
const expect = chai.expect;

const icc_post_labconfirm_controller = require("./endpoint_controllers/icc_post_labconfirm_controller");
const app_post_register_controller = require("./endpoint_controllers/app_post_register_controller");
const app_post_labverify_controller = require("./endpoint_controllers/icc_post_labverify_controller");
const env = require("../../util/env_config");
const padding = require("../../util/app_register_padding_data");
const bearer = require("../../util/icc_bearer_token_data");

let apiResponse, content,pollToken,labConfirmationId;

describe('Labverify endpoints tests #labverify #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        return app_post_register_controller.register(env.REGISTER,padding).then(function (register){
            labConfirmationId = register.response.data.labConfirmationId;
        }).then(function (){
            return icc_post_labconfirm_controller.labconfirm(env.LABCONFIRM,labConfirmationId,bearer).then(function (response){
                pollToken = response.data.pollToken;
                });
        }).then(function (){
            return app_post_labverify_controller.labverify(env.LABVERIFY,pollToken,bearer).then(function (response){
                apiResponse = response;
                content = apiResponse.data;
            });
        });
    });

    it('A labverify response structure validation', function (){
        expect(apiResponse.status,'response status code').to.be.eql(200);
        expect(apiResponse.headers,'response header').to.have.nested.property("content-type","application/json; charset=utf-8");

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

    });

    it('Labverify has all needed property keys', function () {
        expect(content).to.have.nested.property("valid");
        expect(content).to.have.nested.property("pollToken");
    });

    it('Labverify response is values', function (){

        if(content.valid==true){
            expect(content.pollToken).to.be.not.null;
        }else {
            expect(content.valid).to.be.false;
            expect(content.pollToken).to.be.null;
        }
    });

    it('Labverify response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });
});