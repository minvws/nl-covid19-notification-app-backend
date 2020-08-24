const dataprovider = require("../data/dataprovider");
const lab_verify_controller = require("../endpoint_controllers/icc_post_labverify_controller");
const bearer = require("../../util/icc_bearer_token_data");
const env = require("../../util/env_config");

async function lab_verify(pollToken) {
  return await lab_verify_controller(env.LABVERIFY,pollToken, bearer);
}

module.exports = lab_verify;