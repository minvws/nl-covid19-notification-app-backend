const decode_protobuf = require("../util/protobuff_decoding");
const axios = require("axios");
const env = require("../util/env_config");
const dataprovider = require("../test/data/dataprovider");

async function test_proto_buff() {
  // TST:
  // werkt niet
  // const exposurehash =
  //   "5eddcea45161995cec6677e19e801f123e6df66f35922ae0be03464fb18509ac";
  //
  // werken wel
  const exposurehash =
    "5c663dbdd2c8e63c84781b7b69a030b5b3021f05054d2893bff3b5670edf9e83";
  // const exposurehash =
  //   "dd6204d7e8e877e5cc22e029e7789044aa3fde89c36e164221989edefef89476";

  // ACC:
  // Werkt niet:
  // const exposurehash =
  //   //   "1077b1c36fedbf36c72ff813ca40d3edcfd90417a992b0474dd503b52104f7ac";
  //   // "1dc577eaf64ac359df352fd4907e71247eab2d646203b227aa37df2e68b5a75f";
  //   "85ada4c38a4cedd8f489a3124d95ecb1d84e3a25eac49f08753e118f4a04ccb9";
  // "e54734da15eaa552825285ef4ccb12180e1981420e7d41d0cc7844047b8b2652";
  // "e3e7f96e5677298e67706be60b593ace6b44b439a67b8102588a76a3f2c767eb";
  // "c57fb805a9af434855664950b739a8b65d88e32d12d561ac5c1b4ea80a83cc54";
  // "9cc02084d37b1a91483d84f328556ea498afcd97d691e2c72ba2c7b78a5c7bcb";

  // werkt wel:

  const baseurl1 = env.EXPOSUREKEYSET; //  "https://vwspa-cdn-blob.azureedge.net";
  const url = `${baseurl1}/${exposurehash}`;
  let payload = dataprovider(
    "post_keys_payload",
    "payload",
    "valid_dynamic",
    new Map()
  );
  console.log(payload);

  let promise = new Promise(function (resolve, reject) {
    axios({
      method: "get",
      url: url,
      responseType: "stream",
    }).then(function (response) {
      decode_protobuf(response).then(function (EKS) {
        let exposure_key_send = JSON.parse(
          dataprovider(
            "post_keys_payload",
            "payload",
            "valid_dynamic",
            new Map()
          )
        ).keys[0].keyData;

        for (key of EKS.keys) {
          console.log(key.keyData.toString("base64"));
          console.log(exposure_key_send);
          console.log(key.keyData.toString("base64").equals(exposure_key_send));
        }
        console.log(EKS.keys);
        resolve(EKS);
      });
    });
  });
  return promise;
}

test_proto_buff();
