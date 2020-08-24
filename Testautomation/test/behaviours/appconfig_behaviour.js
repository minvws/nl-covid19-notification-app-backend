const dataprovider = require("../data/dataprovider");
const app_config_controller = require("../endpoint_controllers/cdn_get_appconfig_controller");
const env = require("../../util/env_config");

async function appconfig(appConfigId) {
  return await app_config_controller(env.APPCONFIG,appConfigId);
}

module.exports = appconfig;