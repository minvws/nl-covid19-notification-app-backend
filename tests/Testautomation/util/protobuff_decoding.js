const fs = require("fs");
var protobufjs = require("protobufjs");
const AdmZip = require("adm-zip");

function decode_proto_buf(response){
  return new Promise(function (resolve,reject){
    const input_file_path = __dirname + "/temp/local_eks.zip";
    response.data.pipe(fs.createWriteStream(input_file_path)).on("close", async function (){
      const zip = new AdmZip(input_file_path);
      let zipEntries = zip.getEntries();
      protobufjs.load(__dirname + "/export.proto",  function (err, root) {
        if (err) throw err;
        const bin = zipEntries[0].getData().slice(12);
        let messageDecode = root.lookupType("TemporaryExposureKeyExport");
        resolve(messageDecode.decode(bin));
      });
    });


  })
}

module.exports = decode_proto_buf;
