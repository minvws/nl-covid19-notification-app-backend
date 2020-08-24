const dataprovider = require("../data/dataprovider");
const exposure_key_set_controller = require("../endpoint_controllers/cdn_get_exposurekeyset_controller");
const env = require("../../util/env_config");

async function exposure_key_set(exposureKeySetId) {
  return await exposure_key_set_controller(
      env.EXPOSUREKEYSET,
      exposureKeySetId
  );
}

module.exports = exposure_key_set;