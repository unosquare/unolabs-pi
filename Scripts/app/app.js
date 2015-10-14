(function() {
    'use strict';

    angular.module('app', [
        'ngRoute',
        'tubular',
        'ui.bootstrap'
    ]).config([
        '$httpProvider', function($httpProvider) {
            $httpProvider.interceptors.push('noCacheInterceptor');
        }
    ]).config([
        '$routeProvider', '$locationProvider', function($routeProvider, $locationProvider) {
            $routeProvider.
                when('/', {
                    templateUrl: '/ui/views/home.html',
                    title: 'Home',
                    print: true
                }).when('/Camera', {
                    templateUrl: '/ui/views/photo.html',
                    title: 'Camera'
                }).when('/IO', {
                    templateUrl: '/ui/views/io.html',
                    title: 'Pin I/O'
                }).otherwise({
                    redirectTo: '/'
                });

            $locationProvider.html5Mode(true);
        }
    ]).factory('noCacheInterceptor', function() {
        return {
            request: function(config) {
                if (config.method == 'GET' && config.url.indexOf('.htm') === -1 && config.url.indexOf('blob:') === -1) {
                    var separator = config.url.indexOf('?') === -1 ? '?' : '&';
                    config.url = config.url + separator + 'noCache=' + new Date().getTime();
                }
                return config;
            }
        };
    }).service('alerts', [
        '$filter', function alerts($filter) {
            var me = this;

            me.previousError = '';

            me.defaultErrorHandler = function(error) {
                var errorMessage = $filter('errormessage')(error);

                // Ignores same error
                if (me.previousError == errorMessage) return;

                me.previousError = errorMessage;

                // Ignores Unauthorized error because it's redirecting to login
                if (errorMessage != "Unauthorized") {
                    toastr.error(errorMessage);
                }
            };
        }
    ]).controller('GenericCtrl', [
        '$scope', 'alerts', '$routeParams', function($scope, alerts, $routeParams) {
            $scope.senderKey = $routeParams.senderkey || '';

            $scope.$on('tbForm_OnConnectionError', function(ev, error) { alerts.defaultErrorHandler(error); });
            $scope.$on('tbGrid_OnConnectionError', function (ev, error) { alerts.defaultErrorHandler(error); });
        }
    ]).controller('IOCtrl', ['$scope', 'tubularHttp', function ($scope, tubularHttp) {
        $scope.IO = function (n) {
            tubularHttp.setRequireAuthentication(false);
            tubularHttp.get('/api/io/' + n).promise.then(function(data) {
                toastr.success('Check IO ' + n);
            });
        };

        $scope.buttonIO = function (n) {
            tubularHttp.setRequireAuthentication(false);
            tubularHttp.get('/api/buttonio/' + n).promise.then(function (data) {
                if (data) toastr.success('Button IO ' + data.count);
            });
        };
    }]).controller('PhotoCtrl', ['$scope', 'tubularHttp', '$timeout', '$modal', function ($scope, tubularHttp, $timeout, $modal) {
        $scope.loadPhotos = function () {
            tubularHttp.setRequireAuthentication(false);
            tubularHttp.get('/api/photos').promise.then(function(data) {
                $scope.items = data;

                $timeout($scope.loadPhotos, 1000);
            });
        };

        $scope.takePhoto = function () {
            tubularHttp.setRequireAuthentication(false);
            tubularHttp.get('/api/takephoto').promise.then(function (data) {
                $scope.items = data;
            });
        };

        $scope.popup = function(item) {
            $modal.open({
                template: '<div style="min-height: 400px; padding: 10px;"><img src="/ui/' + item + '" style="width: 100%; margin: auto;" /></div>'
            });
        };

        $timeout($scope.loadPhotos, 500);
    }]).controller('TitleCtrl', [
        '$scope', '$route', '$location', 'tubularHttp', '$routeParams',
        function($scope, $route, $location, tubularHttp, $routeParams) {
            var me = this;
            me.content = "Sample";
            me.pageTitle = "Loading . . .";
            me.key = "Loading . . .";

            $scope.$on('$routeChangeSuccess', function () {
                $scope.subheader = null;

                me.key = $route.current.title;
                me.pageTitle = me.key;
                if ($routeParams.param) me.pageTitle += " - " + $routeParams.param;
                me.content = me.pageTitle + " - Sample";
            });
        }
    ]).controller('NavCtrl', [
        '$scope', '$route', function ($scope, $route) {
            $scope.hightlight = "";
            $scope.showMenu = false;

            $scope.$on('$routeChangeSuccess', function () {
                $scope.hightlight = $route.current.title;

                $scope.showMenu = $route.current.title !== 'Login';
            });

            $scope.menu = [
                { icon: 'fa-dashboard', title: 'Home', path: '/' },
                { icon: 'fa-camera', title: 'Camera', path: '/Camera' },
                { icon: 'fa-bolt', title: 'Pin I/O', path: '/IO' }
            ];
        }
    ]);
})();