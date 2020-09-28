const dataprovider = require("../data/dataprovider");
const app_config_controller = require("../endpoint_controllers/cdn_get_appconfig_controller");
const env = require("../../util/env_config");

async function appconfig(appConfigId, version) {
  let str = env.APPCONFIG
  let endpoint = str.replace("v1",version);
  return await app_config_controller(endpoint,appConfigId);
}

module.exports = appconfig;