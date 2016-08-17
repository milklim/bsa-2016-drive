﻿(function () {
    'use strict';

    angular
        .module('driveApp')
        .factory('SpaceService', SpaceService);

    SpaceService.$inject = ['$http'];

    function SpaceService($http) {
        var service = {
            getSpace: getSpace,
            getAllSpaces: getAllSpaces,
            searchFoldersAndFiles,
            getNumberOfResultSearchFoldersAndFiles,
            getSpaceTotal
        };

        function getSpace(id, currentPage, pageSize, callback) {
            $http.get('/api/spaces/' + id, {
                params: {
                    page: currentPage,
                    count: pageSize
                }
            })
                .then(function (response) {
                    if (callback) {
                        callback(response.data);
                    }
                }, function () {
                    console.log('Error getSpace spaceService!');
                });
        }

        function getSpaceTotal(id, callback) {
            $http.get('/api/spaces/' + id + '/sptotal')
               .then(function (response) {
                   if (callback) {
                       callback(response.data);
                   }
               }, function () {
                   console.log('Error getSpaceTotal spaceService!');
               });

        }

        function getAllSpaces(callback) {
            $http.get('/api/spaces')
            .then(function (response) {
                if (callback) {
                    callback(response.data);
                }
            }, function () {
                console.log('Error while getting all spaces!');
            });
        }

        function searchFoldersAndFiles(spaceId, folderId, text, currentPage, pageSize, callback) {
            $http.get('/api/spaces/' + spaceId + '/search', {
                params: {
                    folderId: folderId,
                    text: text,
                    page: currentPage,
                    count: pageSize
                }
            })
            .then(function (response) {
                if (callback) {
                    callback(response.data);
                }
            }, function () {
                console.log('Error in searchFoldersAndFiles Method!');
            });
        }

        function getNumberOfResultSearchFoldersAndFiles(spaceId, folderId, text, callback) {
            $http.get('/api/spaces/' + spaceId + '/total', {
                params: {
                    folderId: folderId,
                    text: text
                }
            })
            .then(function (response) {
                if (callback) {
                    callback(response.data);
                }
            }, function () {
                console.log('Error in getNumberOfResultSearchFoldersAndFiles Method!');
            });
        }

        return service;
    }

    var app = angular.module('driveApp');

    app.filter('typeOfFile', function () {
        return function (input, uppercase) {
            switch (input) {
                case 0:
                    return 'None';
                case 1:
                    return 'Document';
                case 2:
                    return 'Archive';
                case 3:
                    return 'Presentation';
                case 4:
                    return 'WebPage';
                default:
                    return '';
            }
        }
    });
})();