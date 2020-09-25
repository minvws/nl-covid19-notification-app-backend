const scen = {
  post_keys_payload: require("./scenario_data/post_keys_payload"),
  lab_confirm_payload: require("./scenario_data/lab_confirm_payload")
};

let saved = new Map();

function get_data(scenario, step, state, map_of_vars) {
  if (saved.has(scenario)) {
    if (saved.get(scenario).has(step)) {
      return saved.get(scenario).get(step);
    }
  }
  let base = scen[scenario][step][state];
  if (map_of_vars.size > 0) {
    for (let entry of map_of_vars.entries()) {
      base = base.replace(entry[0], entry[1]);
    }
  }
  saved.set(scenario, new Map().set(step, base));
  return base;
}

function clear_saved(){
  saved.clear();
}

module.exports = {
  get_data: get_data,
  clear_saved: clear_saved
}
