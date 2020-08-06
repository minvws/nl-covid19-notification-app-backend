const axios = require("axios");

let postkeys = function (endpoint,padding, bucketId, sig) {
  //
  // console.log(endpoint);
  // console.log(padding);
  // console.log(bucketId);
  // console.log(sig);

  bucketId = bucketId.replace(/"/g,'');
  bucketId = bucketId.replace(/,$/g,'');

  let payload = {
    "bucketId": bucketId,
    "keys": [
      {
        "rollingStartNumber": 1234,
        "keyData": "EaMR2wpMuSMMw3wSy32HEQ==",
        "rollingPeriod": 144
      }
    ],
    "padding": padding
  };

  let data = JSON.stringify(payload);

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
      url: endpoint + "?sig=" + sig,
      headers: { Accept: "application/json" },
      data: data,
    })
      .then(function (response) {
        resolve(response);
      })
      .catch(function (error) {
        reject(error);
      });
  });
};

exports.postkeys = postkeys;
