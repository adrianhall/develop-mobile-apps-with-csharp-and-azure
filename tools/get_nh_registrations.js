#!/usr/bin/env node

const CryptoJS = require('crypto-js');
const program = require('commander');
const request = require('request');

function getSelfSignedToken(targetUri, sharedKey, ruleId, expiresInMins) {
    targetUri = encodeURIComponent(targetUri.toLowerCase()).toLowerCase();

    let expireOnDate = new Date();
    expireOnDate.setMinutes(expireOnDate.getMinutes() + expiresInMins);
    let expiry = Date.UTC(
        expireOnDate.getUTCFullYear(), 
        expireOnDate.getUTCMonth(), 
        expireOnDate.getUTCDate(), 
        expireOnDate.getUTCHours(), 
        expireOnDate.getUTCMinutes(), 
        expireOnDate.getUTCSeconds()
    ) / 1000;

    let signature = encodeURIComponent(CryptoJS.HmacSHA256(`${targetUri}\n${expiry}`, sharedKey).toString(CryptoJS.enc.Base64));

    let token = `SharedAccessSignature sr=${targetUri}&sig=${signature}&se=${expiry}&skn=${ruleId}`;
    return token;
}

function parseConnectionString(connectionString) {
    let parts = connectionString.split(';');
    if (parts.length != 3)
        throw "Error parsing connection string";

    let response = {};
    parts.forEach(function(part) {
        if (part.indexOf('Endpoint') == 0) {
            response.endpoint = 'https' + part.substring(11);
        } else if (part.indexOf('SharedAccessKeyName') == 0) {
            response.sasKeyName = part.substring(20);
        } else if (part.indexOf('SharedAccessKey') == 0) {
            response.sasKeyValue = part.substring(16);
        }
    });

    return response;
}

function parseXML(xml, callback) {
    const handler = new FeedHandler(callback, {
        // No options
    });
    try {
        const parser = new htmlparser2.Parser(handler, { xmlMode: true });
        parser.parseComplete(xml);
    } catch (ex) {
        callback(ex);
    }
}

program
    .version('0.0.1')
    .option('-c, --connectionstring [connectionstring]', 'The Connection String for the Hub')
    .option('-h, --hub [hubname]', 'The Notification Hub')
    .parse(process.argv);

if (program.connectionstring && program.hub) {
    const p = parseConnectionString(program.connectionstring);
    const uri = `${p.endpoint}${program.hub}/registrations?api-version=2015-01`;
    const  options = {
        url: uri,
        headers: {
            'Authorization': getSelfSignedToken(uri, p.sasKeyValue, p.sasKeyName, 10),
            'x-ms-version': '2015-01'
        }
    };
    request(options, function (error, response, body) {
        if (!error && response.statusCode === 200) {
            console.log(body);
        }
    });
} else {
    console.error('Usage: ${process.argv[1]} -c <connection string> -h <hub name>');
}
