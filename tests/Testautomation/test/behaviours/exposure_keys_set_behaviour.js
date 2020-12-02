const dataprovider = require("../data/dataprovider");
const exposure_key_set_controller = require("../endpoint_controllers/cdn_get_exposurekeyset_controller");
const env = require("../../util/env_config");

async function exposure_key_set(exposureKeySetId, version) {
  let str = env.EXPOSUREKEYSET
  let endpoint = str.replace("v1",version);
  return await exposure_key_set_controller(endpoint,exposureKeySetId);
}

module.exports = exposure_key_set;