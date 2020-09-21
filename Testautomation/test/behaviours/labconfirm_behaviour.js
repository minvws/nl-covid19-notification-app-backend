const dataprovider = require("../data/dataprovider");
const labconfirm_controller = require("../endpoint_controllers/icc_post_labconfirm_controller");
const bearer = require("../../util/icc_bearer_token_data");
const env = require("../../util/env_config");

async function labconfirm(payload) {
  return await labconfirm_controller(env.LABCONFIRM,payload, bearer);
}

module.exports = labconfirm;