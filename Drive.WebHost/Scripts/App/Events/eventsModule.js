﻿(function () {
    angular.module('driveApp.events', [
        "ang-drag-drop",
        'youtube-embed',
        "dndLists"
    ])
    .config([
        '$routeProvider', function config($routeProvider) {
            var baseUrl = window.globalVars.baseUrl;
            $routeProvider
                .when('/apps/events',
                {
                    templateUrl: baseUrl + '/Scripts/App/Events/List/EventsList.html',
                    controller: 'EventsListController',
                    controllerAs: 'eventsListCtrl'
                })
                .when('/apps/events/newevent',
                {
                    templateUrl: baseUrl + '/Scripts/App/Events/Event/CreateEvent.html',
                    controller: 'EventCreateController',
                    controllerAs: 'eventCreateCtrl'
                })
                .when('/apps/events/:id',
                {
                    templateUrl: baseUrl + '/Scripts/App/Events/Event/Event.html',
                    controller: 'EventController',
                    controllerAs: 'eventCtrl'
                })
                .when('/apps/events/:id/edit',
                {
                    templateUrl: baseUrl + '/Scripts/App/Events/Event/Event.html',
                    controller: 'EventController',
                    controllerAs: 'eventCtrl'
                });
            }
    ])
    .constant('uiDatetimePickerConfig', {
        dateFormat: 'yyyy-MM-dd HH:mm',
        defaultTime: '10:00:00',
        html5Types: {
            date: 'yyyy-MM-dd',
            'datetime-local': 'yyyy-MM-dd HH:mm',
            'month': 'yyyy-MM'
        },
        initialPicker: 'date',
        reOpenDefault: false,
        enableDate: true,
        enableTime: true,
        buttonBar: {
            show: true,
            now: {
                show: true,
                text: 'Now'
            },
            today: {
                show: true,
                text: 'Today'
            },
            clear: {
                show: true,
                text: 'Clear'
            },
            date: {
                show: true,
                text: 'Date'
            },
            time: {
                show: true,
                text: 'Time'
            },
            close: {
                show: true,
                text: 'Close'
            }
        },
        closeOnDateSelection: true,
        closeOnTimeNow: true,
        appendToBody: false,
        altInputFormats: [],
        ngModelOptions: {},
        saveAs: 'ISO',
        readAs: 'ISO'
    });
})();