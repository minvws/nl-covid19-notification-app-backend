const moment = require("moment");

const data = {
  payload: {
    valid_static: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": 1234,
          "keyData": "EaMR2wpMuSMMw3wSy32HEQ==",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    valid_dynamic_yesterday: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": ${generate_date_rsn(true,-1)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    valid_dynamic_13_keys: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": ${generate_date_rsn(true,-13)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-12)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-11)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-10)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-9)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-8)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-7)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-6)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-5)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-4)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-3)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-2)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        },
        {
          "rollingStartNumber": ${generate_date_rsn(true,-1)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    invalid_dynamic_tomorrow: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": ${generate_date_rsn(true,+1)},
          "keyData": "${generate_key(true)}",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    invalid_key: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": ${generate_date_rsn(false)},
          "keyData": "${generate_key(false)}",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    insufficient: "insufficient data for this scenario",
  },
};

function generate_date_rsn(valid,days){
  days = days || 0;
  let unix_sec = moment().add(days, 'days').unix();
  let rsn = Math.floor(unix_sec / 600);

  if (valid) {
    // console.log("RSN: " + rsn)
    return rsn;
  } else {
    return 0;
  }
}

function generate_key(valid) {
  const random_key = Buffer.from(
    Array.from({ length: 16 }, () => Math.floor(Math.random() * 16), "binary")
  ).toString("base64");

  if (valid) {
    return random_key;
  } else {
    return random_key + "ABC";
  }
}



module.exports = data;
