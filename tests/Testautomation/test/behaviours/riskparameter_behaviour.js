const dataprovider = require("../data/dataprovider");
const riskparameter_controller = require("../endpoint_controllers/cdn_get_riskparameters_controller");
const env = require("../../util/env_config");

async function riskparameter(riskParameterId, version) {
  let str = env.RISKPARAMETERS
  let endpoint = str.replace("v1",version);

  return await riskparameter_controller(endpoint,riskParameterId);
}

module.exports = riskparameter;