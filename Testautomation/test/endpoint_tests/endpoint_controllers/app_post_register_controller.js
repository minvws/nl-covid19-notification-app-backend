const axios = require("axios");

let register = function (endpoint, padding) {
  let data = JSON.stringify(padding);

  return new Promise(function (resolve, reject) {
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

    instance({
      method: "post",
      url: endpoint,
      headers: { Accept: "application/json" },
      data: {
        padding: data,
      },
    })
      .then(function (response) {
        let obj = {
          labConfirmationId: response.data.labConfirmationId,
          bucketId:response.data.bucketId,
          confirmationKey:response.data.confirmationKey,
          validity: response.data.validity,
          response:response
        }
        resolve(obj);
      })
      .catch(function (error) {
        reject(error);
      });
  });
};

exports.register = register;
