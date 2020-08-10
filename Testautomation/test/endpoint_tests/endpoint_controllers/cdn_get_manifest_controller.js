const axios = require("axios");
const AdmZip = require("adm-zip");

let manifest = function (endpoint) {
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
      method: "get",
      url:
        endpoint +
        "?sv=2019-02-02&st=2020-06-26T14%3A13%3A35Z&se=2025-06-27T14%3A13%3A00Z&sr=c&sp=rl&sig=fCwrecfbx6JNp5RlTzw5mmnRCna6hne92Khb8Gk4%2BRw%3D",
      headers: { Accept: "application/json" },
      responseType: "arraybuffer",
      responseEncoding: null,
    })
      .then(function (response) {
        let content = unZip(response.data)
        let obj =
            {content: {
                exposureKeySets: content.exposureKeySets,
                resourceBundle:content.resourceBundle,
                riskCalculationParameters: content.riskCalculationParameters,
                appConfig:content.appConfig
            },
          response:response
        }
        resolve(obj);
      })
      .catch(function (error) {
        reject(error);
      });
  });
};

function unZip(data){
  let zip = new AdmZip(data);
  let zipEntries = zip.getEntries();
  return JSON.parse(zip.readAsText(zipEntries[0]).toString("utf8"));
}

exports.manifest = manifest;
