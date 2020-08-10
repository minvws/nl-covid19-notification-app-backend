const scen = {
  scenA: require("./scenario_data/scenA"),
};

function get_data(state, scenario, step) {
  return scen[scenario][step][state];
}

module.exports = get_data;
