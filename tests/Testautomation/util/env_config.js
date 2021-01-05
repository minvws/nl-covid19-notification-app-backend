let argv = require("minimist")(process.argv.slice(2));

/*
TST
test.coronamelder-dist.nl => /v1/manifest
test.coronamelder-api.nl => /v1/register
test.coronamelder-portal.nl/iccauth/caregiversportal => /v1/labconfirm
test.coronamelder-portal.nl => ICC frontend

https://test.coronamelder-portal.nl
 */

// Set version
let version = argv.version;
if (version == undefined) {
  version = "v1";
}

// Sets default endpoint urls, resources per environment
const endpoints = {
  CDN:{
    MANIFEST: "manifest",
    APPCONFIG: "appconfig",
    EXPOSUREKEYSET: "exposurekeyset",
    RISKPARAMETERS: "riskcalculationparameters",
    RESOURCEBUNDLE: "resourcebundle",
  },
  APP:{
    POSTKEYS: "postkeys",
    STOPKEYS: "stopkeys",
    REGISTER: "register",
  },
  ICC:{
    LABCONFIRM: "labconfirm",
    LABVERIFY: "labverify",
  }

};

const base_url = {
  CDN:{
    DEV: "http://localhost:5004",
    TST: "https://test.coronamelder-dist.nl",
    ACC: "https://acceptatie.coronamelder-dist.nl",
    PROD: "https://productie.coronamelder-dist.nl",
  },
  APP:{
    DEV: "http://localhost:5005",
    TST: "https://test.coronamelder-api.nl",
    ACC: "https://acceptatie.coronamelder-api.nl",
    PROD: "https://coronamelder-api.nl",
  },
  ICC:{
    DEV: "http://localhost:5006",
    TST: "https://test.coronamelder-portal.nl/iccauth/CaregiversPortalApi",
    ACC: "https://acceptatie.coronamelder-portal.nl/iccauth/CaregiversPortalApi",
    PROD: "https://coronamelder-portal.nl/iccauth/CaregiversPortalApi",
  }
};

const environment = {
  DEV: {
    MANIFEST: base_url.CDN.DEV + "/" + version + "/" + endpoints.CDN.MANIFEST,
    APPCONFIG: base_url.CDN.DEV + "/" + version + "/" + endpoints.CDN.APPCONFIG,
    EXPOSUREKEYSET: base_url.CDN.DEV + "/" + version + "/" + endpoints.CDN.EXPOSUREKEYSET,
    RISKPARAMETERS: base_url.CDN.DEV + "/" + version + "/" + endpoints.CDN.RISKPARAMETERS,
    RESOURCEBUNDLE: base_url.CDN.DEV + "/" + version + "/" + endpoints.CDN.RESOURCEBUNDLE,
    POSTKEYS: base_url.APP.DEV + "/" + version + "/" + endpoints.APP.POSTKEYS,
    STOPKEYS: base_url.APP.DEV + "/" + version + "/" + endpoints.APP.STOPKEYS,
    REGISTER: base_url.APP.DEV + "/" + version + "/" + endpoints.APP.REGISTER,
    LABCONFIRM: base_url.ICC.DEV + "/" + version + "/" + endpoints.ICC.LABCONFIRM,
    LABVERIFY: base_url.ICC.DEV + "/" + version + "/" + endpoints.ICC.LABVERIFY,
  },
  TST: {
    MANIFEST: base_url.CDN.TST + "/" + version + "/" + endpoints.CDN.MANIFEST,
    APPCONFIG: base_url.CDN.TST + "/" + version + "/" + endpoints.CDN.APPCONFIG,
    EXPOSUREKEYSET: base_url.CDN.TST + "/" + version + "/" + endpoints.CDN.EXPOSUREKEYSET,
    RISKPARAMETERS: base_url.CDN.TST + "/" + version + "/" + endpoints.CDN.RISKPARAMETERS,
    RESOURCEBUNDLE: base_url.CDN.TST + "/" + version + "/" + endpoints.CDN.RESOURCEBUNDLE,
    POSTKEYS: base_url.APP.TST + "/" + version + "/" + endpoints.APP.POSTKEYS,
    STOPKEYS: base_url.APP.TST + "/" + version + "/" + endpoints.APP.STOPKEYS,
    REGISTER: base_url.APP.TST + "/" + version + "/" + endpoints.APP.REGISTER,
    LABCONFIRM: base_url.ICC.TST + "/" + version + "/" + endpoints.ICC.LABCONFIRM,
    LABVERIFY: base_url.ICC.TST + "/" + version + "/" + endpoints.ICC.LABVERIFY,
  },
  ACC: {
    MANIFEST: base_url.CDN.ACC + "/" + version + "/" + endpoints.CDN.MANIFEST,
    APPCONFIG: base_url.CDN.ACC + "/" + version + "/" + endpoints.CDN.APPCONFIG,
    EXPOSUREKEYSET: base_url.CDN.ACC + "/" + version + "/" + endpoints.CDN.EXPOSUREKEYSET,
    RISKPARAMETERS: base_url.CDN.ACC + "/" + version + "/" + endpoints.CDN.RISKPARAMETERS,
    RESOURCEBUNDLE: base_url.CDN.ACC + "/" + version + "/" + endpoints.CDN.RESOURCEBUNDLE,
    POSTKEYS: base_url.APP.ACC + "/" + version + "/" + endpoints.APP.POSTKEYS,
    STOPKEYS: base_url.APP.ACC + "/" + version + "/" + endpoints.APP.STOPKEYS,
    REGISTER: base_url.APP.ACC + "/" + version + "/" + endpoints.APP.REGISTER,
    LABCONFIRM: base_url.ICC.ACC + "/" + version + "/" + endpoints.ICC.LABCONFIRM,
    LABVERIFY: base_url.ICC.ACC + "/" + version + "/" + endpoints.ICC.LABVERIFY,
  },
  PROD: {
    MANIFEST: base_url.CDN.PROD + "/" + version + "/" + endpoints.CDN.MANIFEST,
    APPCONFIG: base_url.CDN.PROD + "/" + version + "/" + endpoints.CDN.APPCONFIG,
    EXPOSUREKEYSET: base_url.CDN.PROD + "/" + version + "/" + endpoints.CDN.EXPOSUREKEYSET,
    RISKPARAMETERS: base_url.CDN.PROD + "/" + version + "/" + endpoints.CDN.RISKPARAMETERS,
    RESOURCEBUNDLE: base_url.CDN.PROD + "/" + version + "/" + endpoints.CDN.RESOURCEBUNDLE,
    POSTKEYS: base_url.APP.PROD + "/" + version + "/" + endpoints.APP.POSTKEYS,
    STOPKEYS: base_url.APP.PROD + "/" + version + "/" + endpoints.APP.STOPKEYS,
    REGISTER: base_url.APP.PROD + "/" + version + "/" + endpoints.APP.REGISTER,
    LABCONFIRM: base_url.ICC.PROD + "/" + version + "/" + endpoints.ICC.LABCONFIRM,
    LABVERIFY: base_url.ICC.PROD + "/" + version + "/" + endpoints.ICC.LABVERIFY,
  }
};

// Set environment
let env = environment[argv.environment];
if (env == undefined) {
  env = environment.TST;
}

module.exports = env;
