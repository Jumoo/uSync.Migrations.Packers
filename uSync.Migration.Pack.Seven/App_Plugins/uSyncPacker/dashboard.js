(function () {
    'use strict';

    function packerDashboard($scope,
        notificationsService,
        uSyncMigrationPackService) {

        var vm = this;
        vm.makePack = makePack;
        vm.state = 'init';
        vm.stage = '';


        var methods = [
            {
                method: uSyncMigrationPackService.getConfig,
                name: 'Get Config'
            },
            {
                method: uSyncMigrationPackService.copyViews,
                name: 'Copy views'
            },
            {
                method: uSyncMigrationPackService.copyFiles,
                name: 'Copy Files'
            }
        ];

        function makePack() {
            vm.state = 'busy';
            vm.stage = 'exporting';
            uSyncMigrationPackService.createExport()
                .then(function (response) {
                    var id = response.data;
                    process(id, 0);
                }, function (error) {
                    vm.state = 'error';
                    console.log('error', error);
                    notificationsService.error('error', error.ExceptionMessage);
                });
        }

        function process(id, index) {

            if (methods.length >  index) {

                vm.step = methods[index].name;
                methods[index].method(id)
                    .then(function (response) {
                        process(id, index++);
                    }, function (error) {
                        vm.state = 'error';
                        console.log('error', error);
                        notificationsService.error('error', error.ExceptionMessage);
                    });
            }
            else {
                // done...
                download(id);
            }
        }

        function download(id) {
            uSyncMigrationPackService.zipExport()
                .then(function (response) {
                    vm.state = 'success';
                    notificationsService.success('packed', 'pack created');
                }, function (error) {
                    vm.state = 'error';
                    console.log('error', error);
                    notificationsService.error('error', error.ExceptionMessage);
                });
        }

    }

    angular.module('umbraco')
        .controller('uSyncPackerDashboardController', packerDashboard);
})();