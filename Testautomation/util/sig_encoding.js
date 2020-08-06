const fs = require("fs");
const exec = require('child_process').exec;

let testsSig = function (bucketId,padding,cid){
    return new Promise(function (resolve,reject){

        // bucketId = "AxIE73ZlE60I3JbNoT0JiPO41sGMagxLvmYIpxYYulk=";
        // padding = "Yg==";
        // cid = "YI42ksgvRT2F0DHJhg9t1/15Vpon1L6+xqk3ww6ayvo=";

        bucketId = bucketId.replace(/"/g,'');
        bucketId = bucketId.replace(/,$/g,'');
        cid = cid.replace(/"/g,'');
        cid = cid.replace(/,$/g,'');

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

        let payloadString = JSON.stringify(payload);
        let fileName = __dirname + 'payload'

        const writeFilePromise = (file, data) => {
            return new Promise((resolve, reject) => {
                fs.writeFile(file, data, error => {
                    if (error) reject(error);
                    resolve("done");
                });
            });
        };

        let keyCommand = `echo ${cid} | base64 -d | xxd -p -c 256`;

        const fileCreate = writeFilePromise(fileName,payloadString);
        const sig = fileCreate.then(inputFile => {
            execShellCommand(keyCommand).then(KEY => {
                let sigCommand = `cat ${fileName} | openssl sha256 -mac HMAC -macopt hexkey:${KEY} -binary | base64 | sed -e 's/"//g' -e 's/+/%2B/g' -e 's/=/%3D/g' -e 's/\\//%2F/g`;
                execShellCommand(sigCommand).then(sig => {
                    // this function has access to variables inputFile, Key and sig
                    // console.log(inputFile);
                    // console.log(KEY);
                    // console.log(sig);

                    resolve({sig:sig});
                });
            });
        })
        const fileRemove = sig.then(file => {
            fs.unlink(fileName, (err) => {
                if (err) {
                    console.error(err)
                    return
                }
            });
        })


        function execShellCommand(cmd) {
            return new Promise((resolve, reject) => {
                exec(cmd, (error, stdout, stderr) => {
                    if (error) {
                        // console.warn(error);
                    }
                    resolve(stdout? stdout : stderr);
                })
            })
        };
    });
}

exports.testsSig = testsSig;


