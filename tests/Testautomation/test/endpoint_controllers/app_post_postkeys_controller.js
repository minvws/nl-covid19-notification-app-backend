const axios = require("axios");
const axiosLogger = require("axios-logger");

async function postkeys(endpoint, payload, sig) {

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

  // add logging on request and response
  instance.interceptors.request.use(axiosLogger.requestLogger,axiosLogger.errorLogger);
  instance.interceptors.response.use(axiosLogger.responseLogger,axiosLogger.errorLogger);

  const response = await instance({
    method: "post",
    url: endpoint + "?sig=" + sig,
    headers: { "Accept": "application/json",
      "Content-Type":"application/json"
    },
    data: payload,
  });

  return response;
}

module.exports = postkeys;
