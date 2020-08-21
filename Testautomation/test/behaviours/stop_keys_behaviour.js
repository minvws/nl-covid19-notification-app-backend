const dataprovider = require("../data/dataprovider");
const stopkeys_controller = require("../endpoint_controllers/app_post_postkeys_controller");
const padding = require("../data/scenario_data/app_register_padding_data");
const env = require("../../util/env_config");

async function stop_keys(bucket_id, sig) {
  return await stopkeys_controller(
    env.STOPKEYS,
    padding,
    bucket_id,
    sig
  );
}

module.exports = stop_keys;