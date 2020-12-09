const dataprovider = require("../data/dataprovider");
const resource_bundle_controller = require("../endpoint_controllers/cdn_get_resourcebundle_controller");
const env = require("../../util/env_config");

async function resourcebundle(resourceBundleId, version) {
    let str = env.RESOURCEBUNDLE
    let endpoint = str.replace("v1",version);
    return await resource_bundle_controller(endpoint,resourceBundleId);
}

module.exports = resourcebundle;
