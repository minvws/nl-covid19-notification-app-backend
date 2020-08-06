const fs = require("fs");
var protobuf = require("protobufjs");
const AdmZip = require("adm-zip");
const exec = require('child_process').exec;

let decode_proto_buff = function decode_proto_buff(response, callback) {
  input_file_path = __dirname + "/temp/eks.zip";
  response.data.pipe(fs.createWriteStream(input_file_path));

  fs.readFile(input_file_path, function () {
    const zip = new AdmZip(input_file_path);
    let zipEntries = zip.getEntries();
    protobuf.load(__dirname + "/export.proto", function (err, root) {
        if (err) throw err;
        const bin = zipEntries[0].getData().slice(18);
        console.log(zipEntries[0].entryName);
        console.log(bin.byteLength);
        let messageDecode = root.lookupType("TemporaryExposureKeyExport");
        let buf = Buffer.from(bin, "base64");
        callback(messageDecode.decode(buf));
    });
  });
}

exports.decode_proto_buff = decode_proto_buff;
