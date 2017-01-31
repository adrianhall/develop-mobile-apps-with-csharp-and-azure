#!node
var uuid = require('uuid');
var stdin = process.stdin,
    inputChunks = [];

stdin.resume();
stdin.setEncoding('utf8');
stdin.on('data', function (chunk) {
    inputChunks.push(chunk);
});

stdin.on('end', function() {
    var inputJSON = inputChunks.join(),
        parsedData = JSON.parse(inputJSON),
        newData = { "value": [] };

    parsedData.forEach(function (elem) {
        elem['@search.action'] = 'upload';
        elem['videoId'] = uuid.v4();
        newData.value.push(elem);
    });

    process.stdout.write(JSON.stringify(newData, null, '    '));
    process.stdout.write('\n');
});
