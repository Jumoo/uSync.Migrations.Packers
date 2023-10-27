/// <binding ProjectOpened='default' />
const { watch, src, dest } = require('gulp');

const sources = [
    'uSync.Migration.Packers.Shared/App_Plugins'
];

const destinations = [
    'MigrationPackers.v8.Site/App_Plugins/',
    'MigrationPackers.v7.Site/App_Plugins/'
];

function copy(path, base) {

    destinations.forEach(function (target) {
        console.log(time(), path.substring(base.length + 1), '>', target.substring(0, target.indexOf('/')));

        src(path, { base: base })
            .pipe(dest(target));
    });
}

function time() {
    return '[' + new Date().toISOString().slice(11, -5) + ']';
}

exports.default = function () {

    sources.forEach(function (source) {

        var searchPath = source + '/**/*';

        watch(searchPath, { ignoreInitial: false })
            .on('change', function (path, stats) {
                copy(path, source);
            })
            .on('add', function (path, stats) {
                copy(path, source);
            });
    });
};






