var indexApp = angular.module('indexApp', ['ui.bootstrap']);

indexApp.controller('IndexController', function ($scope, $modal, $log, IndexService) {

    $scope.Keywords = [];
    $scope.Reviews = [];
    $scope.Review = {};

    $scope.refreshData = function (beginDate, endDate) {

        $scope.getKeywords(beginDate, endDate);
        $scope.getReviews(beginDate, endDate);

    };

    $scope.getKeywords = function (beginDate, endDate) {

        if (beginDate == null)
            beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        if (endDate == null)
            endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");

        console.log(beginDate);
        console.log(endDate);

        IndexService
            .requestKeywords(beginDate, endDate)
            .then(function (keywords) {
                $scope.Keywords = keywords;
            });
    };

    $scope.getReviews = function (beginDate, endDate) {

        if (beginDate == null)
            beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        if (endDate == null)
            endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");

        console.log(beginDate);
        console.log(endDate);

        IndexService
            .requestReviews(beginDate, endDate)
            .then(function (reviews) {
                $scope.Reviews = reviews;
            });
    };

    $scope.getReviewsByKeyword = function (keyword) {

        var beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        var endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");

        console.log(beginDate);
        console.log(endDate);
        console.log(keyword);

        IndexService
            .requestReviewsByKeyword(beginDate, endDate, keyword)
            .then(function (reviews) {
                $scope.Reviews = reviews;
            });
    };

    $scope.getReview = function (articleAutoId, callbackFunc) {

        console.log(articleAutoId);

        IndexService
            .requestReview(articleAutoId)
            .then(function (review) {

                $scope.Review = review;
                callbackFunc();

            });
    };

    $scope.showReview = function (articleId) {

        console.log(articleId);

        $scope.getReview(articleId, $scope.popupReview);
    };

    $scope.popupReview = function () {

        var modalInstance = $modal.open({
            templateUrl: 'articleContent.html',
            controller: 'ModalController',
            size: 'lg',
            resolve: {
                data: function () {
                    return $scope.Review;
                }
            }
        });

        modalInstance.result.then(function () {
            // TODO
        }, function () {
            $log.info('Modal dismissed at: ' + new Date());
        });

    };

    var today = (new Date()).toJSON().slice(0, 10);
    $scope.refreshData(today, today);
});

indexApp.controller('ModalController', function ($scope, $modalInstance, $sce, data) {

    $scope.data = data;

    $scope.ok = function () {
        $modalInstance.close('ok');
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    $scope.renderHtml = function (htmlCode) {
        return $sce.trustAsHtml(htmlCode);
    };
});

indexApp.service('IndexService', function ($http, $q) {

    return ({
        requestKeywords: requestKeywords,
        requestReviews: requestReviews,
        requestReviewsByKeyword: requestReviewsByKeyword,
        requestReview: requestReview,
    });

    function requestKeywords(beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/keywords/" + beginDate + "/" + endDate),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReviews(beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/reviews/" + beginDate + "/" + endDate),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReviewsByKeyword(beginDate, endDate, keyword) {
        var request = $http({
            method: "get",
            url: ("/reviewsByKeyword/" + beginDate + "/" + endDate + "/" + keyword),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReview(articleAutoId) {
        var request = $http({
            method: "get",
            url: ("/review/" + articleAutoId),
        });

        return (request.then(handleSuccess, handleError));
    }

    function handleSuccess(response) {
        return (response.data);
    }

    function handleError(response) {
        if (!angular.isObject(response.data) || !response.data.message) {
            return ($q.reject("An unknown error occurred."));
        }

        // Otherwise, use expected error message.
        return ($q.reject(response.data.message));
    }
});
