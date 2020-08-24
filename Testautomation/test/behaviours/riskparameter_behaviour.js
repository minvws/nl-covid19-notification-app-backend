const dataprovider = require("../data/dataprovider");
const riskparameter_controller = require("../endpoint_controllers/cdn_get_riskparameters_controller");
const env = require("../../util/env_config");

async function riskparameter(riskParameterId) {
  return await riskparameter_controller(env.RISKPARAMETERS,riskParameterId);
}

module.exports = riskparameter;