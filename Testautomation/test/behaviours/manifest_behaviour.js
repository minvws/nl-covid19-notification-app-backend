const dataprovider = require("../data/dataprovider");
const manifest_controller = require("../endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");

async function manifest() {
  return await manifest_controller(env.MANIFEST);
}

module.exports = manifest;