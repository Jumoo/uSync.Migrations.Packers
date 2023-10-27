(function () {
    'use static';

    function dashboardController(migrationPackService,
        notificationsService)
    {
        var vm = this;
        vm.state = 'init';
        vm.loading = true;
        vm.working = false;
        vm.current = 'working';

        vm.makePack = makePack;

        checkForContentEdition();

        function checkForContentEdition() {
            migrationPackService.hasContentEdition()
                .then(function (response) {
                    vm.hasContentEdition = response.data;
                    vm.loading = false; 
                });
        }

        function makePack() {
            vm.state = 'busy';
            vm.working = true;
            vm.percent = 0;
            packStep('00000000-0000-0000-0000-000000000000', 0);
        }

        function packStep(id, index)
        {
            progress(index);

            migrationPackService.packStep(id, index)
                .then(function (result) {
                    var response = result.data;
                    id = response.id;
                    if (response.complete) {
                        // done.
                        // download it now. 
                        getPack(id);
                    }
                    else {
                        packStep(id, index + 1)
                    }
                }, function (error) {
                    vm.state = 'error';
                    vm.working = false;
                    console.log('error', error);
                    notificationsService.error('error', error.ExceptionMessage);
                });
        }

        function getPack(id) {
            migrationPackService.getPack(id)
                .then(function (result) {
                    vm.state = 'success';
                    vm.working = false;
                    notificationsService.success('packed', 'pack created');
                }, function (error) {
                    vm.state = 'error';
                    console.log('error', error);
                    notificationsService.error('error', error.ExceptionMessage);
                });
        }

        var steps = [
            'Exporting...',
            'Config...',
            'Views...',
            'styles...',
            'packing...',
            '...',
            '...'
        ]

        function progress(index) {
            vm.current = steps[index];
        }


    }

    angular.module('umbraco')
        .controller('migrationPackDashboardController', dashboardController);

})();