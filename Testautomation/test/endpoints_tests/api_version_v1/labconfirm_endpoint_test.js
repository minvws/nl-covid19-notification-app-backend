const chai = require("chai");
const expect = chai.expect;
const app_register = require("../../behaviours/app_register_behaviour");
const lab_confirm = require("../../behaviours/labconfirm_behaviour");
const formatter = require("../../../util/format_strings");
const dataprovider = require("../../data/dataprovider");

describe("Lab confirm endpoints tests #labconfirm #endpoints #regression", function () {
  this.timeout(3000 * 60 * 30);

  let app_register_response,
      lab_confirm_response,
      labConfirmationId,
      version = "v1";

  before(function (){
    // Do an API request
    return app_register(version).then(function (register){
      app_register_response = register;
      labConfirmationId = register.data.labConfirmationId;

    }).then(function (){
      let map = new Map();
      map.set("LABCONFIRMATIONID", formatter.format_remove_dash(labConfirmationId));

      return lab_confirm(
          dataprovider.get_data("lab_confirm_payload", "payload", "valid_dynamic_yesterday", map)
          ,version
          ).then(function (confirm){
        lab_confirm_response = confirm;
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

  it('I should get a lab confirmation', function (){
    expect(lab_confirm_response.status,'response status code').to.be.eql(200);
    expect(lab_confirm_response.headers,'response header').to.have.nested.property("content-type","application/json; charset=utf-8");

    let total = (parseFloat(lab_confirm_response.headers["content-length"])/1000).toFixed(3);
    expect(parseInt(total),'Response size below 20KB').to.be.below(20);

  });

  it('Labconfirm has all needed property keys', function () {
    expect(lab_confirm_response.data).to.have.nested.property("valid");
    expect(lab_confirm_response.data).to.have.nested.property("pollToken");
  });

  it('Labconfirm response is values', function (){
    if(lab_confirm_response.data.valid==true){
      expect(lab_confirm_response.data.valid).to.be.true;
      expect(lab_confirm_response.data.pollToken).to.be.not.null;
    }else {
      expect(lab_confirm_response.data.valid).to.be.false;
      expect(lab_confirm_response.data.pollToken).to.be.null;
    }
  });

  it('Labconfirm response time is under 5 sec.', function (){
    expect(lab_confirm_response.headers['request-duration'],"response time is below 5 sec.").to.be.below(5000);
  });

});