const execSync = require('child_process').execSync;
const AdmZip = require("adm-zip");
const fs = require("fs");

function certificate_validation(manifest_response) {
    return new Promise(resolve => {
        // content.sig uit manifest response halen

        const zip = new AdmZip(manifest_response.data);
        const zipEntries = zip.getEntries();
        const content_bin = zip.readAsText(zipEntries[0])
        const content_sig = zip.readAsText(zipEntries[1])
        const G3Root = fs.readFileSync(__dirname + "/G3Root.pem")
        const root = G3Root.toString().replace(/(-----(BEGIN|END) CERTIFICATE-----|[\n\r])/gm, '')

        //
        let optionsB = {
            encoding: 'binary'
        };

        let sigCommand = `openssl cms -verify -CAfile G3Root.pem -in ${content_sig} -inform DER -content ${content_bin} -purpose any`
        console.log(sigCommand)
        const certificate_valid = execSync(sigCommand, optionsB);
        console.log(certificate_valid);

        resolve(certificate_valid);
    })
}

module.exports = certificate_validation;