const dataprovider = require("../data/dataprovider");
const manifest_controller = require("../endpoint_controllers/cdn_get_manifest_controller");
const env = require("../../util/env_config");

async function manifest(version) {
  let str = env.MANIFEST
  let endpoint = str.replace("v1",version);

  return await manifest_controller(endpoint);
}

module.exports = manifest;