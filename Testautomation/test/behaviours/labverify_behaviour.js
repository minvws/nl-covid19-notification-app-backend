const dataprovider = require("../data/dataprovider");
const lab_verify_controller = require("../endpoint_controllers/icc_post_labverify_controller");
const bearer = require("../../util/icc_bearer_token_data");
const env = require("../../util/env_config");

async function lab_verify(pollToken, version) {
  let str = env.LABVERIFY
  let endpoint = str.replace("v1",version);
  return await lab_verify_controller(endpoint,pollToken, bearer);
}

module.exports = lab_verify;