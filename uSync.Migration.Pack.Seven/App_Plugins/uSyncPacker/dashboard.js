(function () {
    'use strict';

    function packerDashboard($scope,
        notificationsService,
        uSyncMigrationPackService) {

        var vm = this;
        vm.makePack = makePack;
        vm.state = 'init';

        function makePack() {
            vm.state = 'busy';

            uSyncMigrationPackService.makePack()
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