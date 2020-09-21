const fs = require("fs");
const exec = require('child_process').exec;
const tmp = require('tmp');
const extraConsoleLogging = false; // switch to true to log debug stuff

let testsSig = function (payload,confirmationKey){
    return new Promise(function (resolve){

        let tmpObj = tmp.fileSync({ mode: 0644, prefix: 'tempfile', postfix: '.txt' });
        let path = tmpObj.name.substring(0, tmpObj.name.lastIndexOf('/'));
        let fileName = tmpObj.name.substring(tmpObj.name.lastIndexOf('/') + 1);

        let keyCommand = `echo ${confirmationKey} | base64 -d | xxd -p -c 256`;
        fs.writeFileSync(tmpObj.name,payload)
        const sig = execShellCommand(keyCommand).then(KEY => {

                let sigCommand = `cat ${tmpObj.name} | openssl sha256 -mac HMAC -macopt hexkey:${KEY} -binary`;
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
                    // tmpObj.removeCallback(); // remove temp file
                    resolve({sig:resultBase64UrlEncode});
                });
            });

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
module.exports = testsSig;