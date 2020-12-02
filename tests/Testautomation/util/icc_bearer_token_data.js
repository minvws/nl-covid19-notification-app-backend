// Set bearer token to login to ICC portal

var args = require('minimist')(process.argv.slice(2));

const bearer = "Bearer " + args.token;

module.exports = bearer;
