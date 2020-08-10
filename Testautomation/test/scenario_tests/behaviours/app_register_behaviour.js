const dataprovider = require("../data/dataprovider");

function register_app(state, scenario) {
  const response_two = register_app_until_step_two(
    state,
    scenario,
    "2",
    response_one
  );
  return (response = appconfig_controller.appconfig(
    dataprovider(state, scenario, "1", response_two)
  ));
}

function register_app_until_step_two() {
  const response_one = register_app_until_step_one(state, scenario, "1");
  return API_CALL_2;
}

function register_app_until_step_one() {
  return API_CALL_1;
}

module.exports = register_app;
