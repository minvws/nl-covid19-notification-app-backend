const axios = require("axios");

async function labconfirm(endpoint, labConfirmationId, bearer) {
  //remove dash from register labcomfirmId
  let labConfirmUpdate = labConfirmationId.replace(/-/g,'');

  let payload = {
    "labConfirmationId": labConfirmUpdate,
    "dateOfSymptomsOnset": "2020-07-23T16:11:19.487Z"
  };

  let data = JSON.stringify(payload);

  const instance = axios.create();
  // add start time header in request
  instance.interceptors.request.use((config) => {
    config.headers["request-startTime"] = process.hrtime();
    return config;
  });

  // add duration header into response
  instance.interceptors.response.use((response) => {
    const start = response.config.headers["request-startTime"];
    const end = process.hrtime(start);
    const milliseconds = Math.round(end[0] * 1000 + end[1] / 1000000);
    response.headers["request-duration"] = milliseconds;
    return response;
  });

  const response = await instance({
    method: "post",
    url: endpoint,
    headers: {
      "Content-Type": "application/json",
      Authorization: bearer,
    },
    data: data,
  });

  return response;

};

module.exports = labconfirm;
