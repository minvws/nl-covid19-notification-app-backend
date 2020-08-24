const fs = require("fs");
const exec = require('child_process').exec;
const padding = require("../test/data/scenario_data/app_register_padding_data");

let testsSig = function (payload,confirmationKey){
    return new Promise(function (resolve,reject){

        let fileName = __dirname + 'payload'

        const writeFilePromise = (file, data) => {
            return new Promise((resolve, reject) => {
                fs.writeFile(file, data, error => {
                    if (error) reject(error);
                    resolve("done");
                });
            });
        };

        let keyCommand = `echo ${confirmationKey} | base64 -d | xxd -p -c 256`;

        const fileCreate = writeFilePromise(fileName,payload);
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