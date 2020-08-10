const chai = require('chai');
const expect = chai.expect;

const app_post_postkeys_controller = require("./endpoint_controllers/app_post_postkeys_controller");
const app_post_register_controller = require("./endpoint_controllers/app_post_register_controller");
const env = require("../../util/env_config");
const padding = require("../../util/app_register_padding_data");
const testsSig = require("../../util/sig_encoding");

let apiResponse,content;

describe('Postkeys endpoints tests #postkeys #endpoints #regression', async function() {
  this.timeout(1000 * 60 * 30);

    before(function (){
        function postKeys (){
            return new Promise(function (resolve){
                let register = app_post_register_controller.register(env.REGISTER,padding);
                let sig = register.then(function(resultRegister) {
                    // console.log(resultRegister);
                    let resultSig = testsSig.testsSig(resultRegister.bucketId,padding,resultRegister.confirmationKey);
                    return resultSig;
                });
                Promise.all([register, sig]).then(function([resultRegister, resultSig]) {
                    // more processing
                    app_post_postkeys_controller.postkeys(env.POSTKEYS,padding,resultRegister.bucketId,resultSig.sig).then(function (response){
                        // console.log(response)
                        return resolve(response);
                    })
                });
            })
        }

        return postKeys().then(function (response){
            apiResponse = response;
            content = apiResponse.data
        });
    });

    it('A postkeys response structure validation', function (){

        expect(apiResponse.status,'response status code').to.be.eql(200);

        let total = (parseFloat(apiResponse.headers["content-length"])/1000).toFixed(3);
        expect(parseInt(total),'Response size below 20KB').to.be.below(20);

    });

    it('A  postkeys response is empty', function (){
        expect(content).to.be.empty;
    })

    it('Postkeys response time is under 200 ms.', function (){
        expect(apiResponse.headers['request-duration']).to.be.below(200);
    });
});