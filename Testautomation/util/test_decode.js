const decode_protobuf = require("../util/protobuff_decoding");
const axios = require("axios");

async function test_proto_buff() {
  const exposurehash =
    "fffbf7d619c860de6abed2aa543a793758942e32c3ca78918e1e1886464eb911";
  const baseurl1 = "https://vwspa-cdn-blob.azureedge.net";
  const url = `${baseurl1}/vws/v01/exposurekeyset/${exposurehash}?sv=2019-02-02&st=2020-06-26T14%3A13%3A35Z&se=2025-06-27T14%3A13%3A00Z&sr=c&sp=rl&sig=fCwrecfbx6JNp5RlTzw5mmnRCna6hne92Khb8Gk4%2BRw%3D`;
  new Promise(function (resolve, reject) {
    axios({
      method: "get",
      url: url,
      responseType: "stream",
    }).then(function (response) {
      decode_protobuf.decode_proto_buff(response, function (a) {
        console.log(a);
        resolve();
      });
    });
  });
}

test_proto_buff();
