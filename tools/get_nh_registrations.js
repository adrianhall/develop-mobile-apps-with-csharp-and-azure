#!/usr/bin/env node

const CryptoJS = require('crypto-js');
const program = require('commander');
const request = require('request');
const FeedMe = require('feedme');

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

const registrationTypes = {
    'appleregistrationdescription': 'APNS (Native)',
    'appletemplateregistrationdescription': 'APNS (Template)',
    'gcmregistrationdescription': 'GCM (Native)',
    'gcmtemplateregistrationdescription': 'GCM (Template)',
    'wnsregistrationdescription': 'WNS (Native)',
    'wnstemplateregistrationdescription': 'WNS (Template)',
    'mpnsregistrationdescription': 'MPNS (Native)',
    'mpnstemplateregistrationdescription': 'MPNS (Template)' 
};

function getRegistrationType(registration) {
    for (let key in registrationTypes) {
        if (typeof registration[key] !== 'undefined') {
            return key;
        }
    }
    return null;
}

function displayContent(registration) {
    const registrationType = getRegistrationType(registration);
    console.log(`Type:         ${registrationTypes[registrationType]}`);
    console.log(`Id:           ${registration[registrationType].registrationid}`);
    console.log(`Device Token: ${registration[registrationType].devicetoken}`);
    console.log(`Tag:          ${registration[registrationType].tags.split(',').join('\nTag:          ')}`);
    console.log(`Expires:      ${registration[registrationType].expirationtime}`);
    console.log('----------------------------------------------------------------------------');
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
            const parser = new FeedMe(true);
            parser.write(body);
            let result = parser.done();
            result.items.forEach(function (item) {
                displayContent(item.content);
            });
        }
    });
} else {
    console.error('Usage: ${process.argv[1]} -c <connection string> -h <hub name>');
}
