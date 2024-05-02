const changeExe = require('changeexe');

const args = process.argv;

(async () => { 
    await changeExe.icon(args[2], args[3]);
})();