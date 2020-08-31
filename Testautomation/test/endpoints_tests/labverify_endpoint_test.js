const chai = require("chai");
const expect = chai.expect;
const app_register = require("../behaviours/app_register_behaviour");
const lab_confirm = require("../behaviours/labconfirm_behaviour");
const lab_verify = require("../behaviours/labverify_behaviour");
const formatter = require("../../util/format_strings");
const dataprovider = require("../data/dataprovider");

describe("Lab verify endpoints tests #labverify #endpoints #regression", function () {
  this.timeout(3000 * 60 * 30);

  let app_register_response, lab_confirm_response, pollToken, lab_verify_response, labConfirmationId;

    before(function (){
      return app_register().then(function (register){
        app_register_response = register;
        labConfirmationId = register.data.labConfirmationId;
      }).then(function (){
        let map = new Map();
        map.set("LABCONFIRMATIONID", formatter.format_remove_dash(labConfirmationId));

        return lab_confirm(dataprovider.get_data("lab_confirm_payload", "payload", "valid_dynamic", map)
        ).then(function (confirm){
          lab_confirm_response = confirm;
          pollToken = confirm.data.pollToken;
        });
      }).then(function (){
        return lab_verify(pollToken).then(function (response){
          lab_verify_response = response;
        });
      });
    });

  after(function (){
    dataprovider.clear_saved();
  })

  it("I should be registered", function () {
    expect(
        app_register_response.status,
        "response status code"
    ).to.be.eql(200);
  });

  it("I should receive lab confirm", function () {
    expect(
        lab_confirm_response.status,
        "response status code"
    ).to.be.eql(200);
  });

  it("I should receive lab verify", function () {
    expect(
        lab_verify_response.status,
        "response status code"
    ).to.be.eql(200);
  });

  it('A labverify response structure validation', function (){
    expect(lab_verify_response.status,'response status code').to.be.eql(200);
    expect(lab_verify_response.headers,'response header').to.have.nested.property("content-type","application/json; charset=utf-8");

    let total = (parseFloat(lab_verify_response.headers["content-length"])/1000).toFixed(3);
    expect(parseInt(total),'Response size below 20KB').to.be.below(20);

  });

  it('Labverify has all needed property keys', function () {
    expect(lab_verify_response.data).to.have.nested.property("valid");
    expect(lab_verify_response.data).to.have.nested.property("pollToken");
  });

  it('Labverify response values', function (){

    if(lab_verify_response.data.valid==true){
      expect(lab_verify_response.data.pollToken).to.be.null;
    }else {
      expect(lab_verify_response.data.valid).to.be.false;
      expect(lab_verify_response.data.pollToken).to.be.not.null;
    }
  });

  it('Labverify response time is under 5 sec.', function (){
    expect(lab_verify_response.headers['request-duration'],"response time is below 5 sec.").to.be.below(5000);
  });

});