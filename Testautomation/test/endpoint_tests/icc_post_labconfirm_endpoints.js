const chai = require('chai');
const expect = chai.expect;

const icc_post_labconfirm_controller = require("./endpoint_controllers/icc_post_labconfirm_controller");
const app_post_register_controller = require("./endpoint_controllers/app_post_register_controller");
const env = require("../../util/env_config");
const padding = require("../../util/app_register_padding_data");
const bearer = require("../../util/icc_bearer_token_data");

let apiResponse, content,labConfirmationId;

describe('Labconfirm endpoints tests #labconfirm #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        // Do an API request
        return app_post_register_controller.register(env.REGISTER,padding).then(function (register){
            labConfirmationId = register.response.data.labConfirmationId;
        }).then(function (){
            return icc_post_labconfirm_controller.labconfirm(env.LABCONFIRM,labConfirmationId,bearer).then(function (response){
                apiResponse = response;
                content = apiResponse.data;
                console.log(content);
                });
        });
    });

    it('A labconfirm response structure validation', function (){
        expect(apiResponse.status,'response status code').to.be.eql(200);
        expect(apiResponse.headers,'response header').to.have.nested.property("content-type","application/json; charset=utf-8");

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

    });

    it('Labconfirm has all needed property keys', function () {
        expect(content).to.have.nested.property("valid");
        expect(content).to.have.nested.property("pollToken");
    });

    it('Labconfirm response is values', function (){
        if(content.valid==true){
            expect(content.valid).to.be.true;
            expect(content.pollToken).to.be.not.null;
        }else {
            expect(content.valid).to.be.false;
            expect(content.pollToken).to.be.null;
        }
    });

    it('Labconfirm response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });
});