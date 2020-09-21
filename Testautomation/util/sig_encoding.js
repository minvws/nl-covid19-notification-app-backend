const fs = require("fs");
const exec = require('child_process').exec;
const padding = require("../test/data/scenario_data/app_register_padding_data");
const extraConsoleLogging = false; // switch to true to log debug stuff

let testsSig = function (payload,confirmationKey){
    return new Promise(function (resolve,reject){

        let fileName = __dirname + '/temp/payload'

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
                let sigCommand = `cat ${fileName} | openssl sha256 -mac HMAC -macopt hexkey:${KEY} -binary`;
                execShellCommand(sigCommand).then(result => {
                    // remove prefix fromm the result
                    var resultHex = result.replace("(stdin)= ", "");
                    var resultBase64 = Buffer.from(resultHex, 'hex').toString('base64');
                    var resultBase64UrlEncode = encodeURIComponent(resultBase64);

                    if(extraConsoleLogging) {
                        console.log('result: ' + result);
                        console.log('resultHex: ' + resultHex);
                        console.log('resultBase64: ' + resultBase64);
                        console.log('resultBase64UrlEncode: ' + resultBase64UrlEncode);
                    }
                    resolve({sig:resultBase64UrlEncode});
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
                        console.warn("!!! ERROR !!!");
                        console.warn(error);
                    }
                    resolve(stdout? stdout : stderr);
                })
            })
        };
    });
}

exports.testsSig = testsSig;