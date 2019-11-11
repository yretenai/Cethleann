var fs = require('fs');
var mdltexlist = fs.readFileSync("Model_and_Texture_List.txt", { encoding: "utf8" }).split('\n');
var csv = fs.readFileSync("filelist.csv", { encoding: "utf8" }).split('\n');

var map = {};

var segment = null;
var counter = 1;
for(var line of mdltexlist) {
    if(line.indexOf('=') == -1) {
        var trim = line.trim();
        if(trim.length > 0) {
            segment = line.trim();
            counter = 1;
        }
        continue;
    }
    var parts = line.trim().split("=");
    if(parts.length < 3) {
        continue;
    }
    var id = parts[1].trim();
    var name = parts[2].trim();
    if(name.toLowerCase() != "null") {
        if(name.length == 0) {
            name = segment + ` ${counter}` 
        } else {
            name = name + ` ${segment}`
        }
        map[id] = name;
    }
    counter += 1;
}

for(var baseLine of csv) {
    var line = baseLine.trim();
    if(line.length == 0 || line.indexOf(',') == -1 || line.startsWith('#')) {
        console.log(baseLine);
        continue;
    }
    var parts = line.split(',');
    var filename = parts[1].split('.', 2);
    if(parts[0] in map && filename.indexOf(' - ') == -1) {
        var name = map[parts[0]];
        console.log(`${parts[0]},${filename[0]} - ${name}.${filename[1]}`)
    } else {
        console.log(baseLine);
    }
}