const dataprovider = require("../data/dataprovider");
const stopkeys_controller = require("../endpoint_controllers/app_post_postkeys_controller");
// const padding = require("../data/scenario_data/app_register_padding_data");
const env = require("../../util/env_config");

async function stop_keys(payload, sig, version) {
  let str = env.STOPKEYS
  let endpoint = str.replace("v1",version);

  return await stopkeys_controller(
    endpoint,
    payload,
    sig
  );
}

module.exports = stop_keys;