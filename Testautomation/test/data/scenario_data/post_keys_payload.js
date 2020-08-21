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
    valid_dynamic: `{
      "bucketId": "BUCKETID",
      "keys": [
        {
          "rollingStartNumber": 1234,
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
          "rollingStartNumber": 1234,
          "keyData": "${generate_key(false)}",
          "rollingPeriod": 144
        }
      ],
      "padding": "Yg=="
    }`,
    insufficient: "insufficient data for this scenario",
  },
};

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
