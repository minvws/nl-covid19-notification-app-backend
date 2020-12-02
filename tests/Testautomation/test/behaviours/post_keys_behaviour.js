const dataprovider = require("../data/dataprovider");
const postkeys_controller = require("../endpoint_controllers/app_post_postkeys_controller");
const padding = require("../data/scenario_data/app_register_padding_data");
const env = require("../../util/env_config");

async function post_keys(payload, sig, version) {
  let str = env.POSTKEYS
  let endpoint = str.replace("v1",version);

  return await postkeys_controller(endpoint,payload,sig);
}

module.exports = post_keys;