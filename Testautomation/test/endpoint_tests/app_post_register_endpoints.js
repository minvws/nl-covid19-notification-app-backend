const chai = require('chai');
const expect = chai.expect;
const app_post_register_controller = require("./endpoint_controllers/app_post_register_controller");
const env = require("../../util/env_config");
const padding = require("../../util/app_register_padding_data");
let apiResponse, content;

describe('Register endpoints tests #register #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        // Do an API request
        return app_post_register_controller.register(env.REGISTER,padding).then(function (response){
            apiResponse = response.response;
            content = apiResponse.data
            console.log(content);
         });
    });

    it('A register response structure validation', function (){
        expect(apiResponse.status,'response status code').to.be.eql(200);
        expect(apiResponse.headers,'response header').to.have.nested.property("content-type","application/json; charset=utf-8");

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

    });

    it('Register has all needed property keys', function () {
        expect(content).to.have.nested.property("labConfirmationId");
        expect(content).to.have.nested.property("bucketId");
        expect(content).to.have.nested.property("confirmationKey");
        expect(content).to.have.nested.property("validity");
    });

    it('Register response is no null values', function (){
        expect(content.labConfirmationId).to.be.not.null;
        expect(content.bucketId).to.be.not.null;
        expect(content.confirmationKey).to.be.not.null;
        expect(content.validity).to.be.not.null;
    });

    it('Register response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });
});