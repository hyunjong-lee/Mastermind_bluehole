var keywordApp = angular.module('keywordApp', ['ui.bootstrap']);

keywordApp.controller('KeywordController', function ($scope, $modal, $log, KeywordService) {

    $scope.Keywords = [];
    $scope.Reviews = [];
    $scope.Review = {};

    $scope.refreshData = function (date) {

        $scope.getKeywords(date);
        $scope.getReviews(date);

    };

    $scope.getKeywords = function(date) {

        if (date == null)
            date = $('#Date').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        console.log(date);

        KeywordService
            .requestKeywords(date)
            .then(function (keywords) {
                $scope.Keywords = keywords;
            });
    };

    $scope.getReviews = function(date) {

        if (date == null)
            date = $('#Date').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        console.log(date);

        KeywordService
            .requestReviews(date)
            .then(function(reviews) {
                $scope.Reviews= reviews;
            });
    };

    $scope.getReviewsByKeyword = function(keyword) {

        var date = $('#Date').data('DateTimePicker').getDate().format("YYYY-MM-DD");

        console.log(date);
        console.log(keyword);

        KeywordService
            .requestReviewsByKeyword(date, keyword)
            .then(function (reviews) {
                $scope.Reviews = reviews;
            });
    };

    $scope.getReview = function (articleAutoId, callbackFunc) {

        console.log(articleAutoId);

        KeywordService
            .requestReview(articleAutoId)
            .then(function (review) {

                $scope.Review = review;
                callbackFunc();

            });
    };

    $scope.showReview = function(articleId) {

        console.log(articleId);

        $scope.getReview(articleId, $scope.popupReview);
    };

    $scope.popupReview = function() {

        console.log("popupReview");

        var modalInstance = $modal.open({
            templateUrl: 'myModalContent.html',
            controller: 'ModalController',
            size: 'lg',
            resolve: {
                data : function () {
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

    $scope.refreshData("2014-10-01");
});

keywordApp.controller('ModalController', function ($scope, $modalInstance, $sce, data) {

    $scope.data = data;

    $scope.ok = function () {
        $modalInstance.close('ok');
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    $scope.renderHtml = function (htmlCode) {

        console.log(htmlCode);

        return $sce.trustAsHtml(htmlCode);
    };
});

keywordApp.service('KeywordService', function($http, $q) {

    return ({
        requestKeywords: requestKeywords,
        requestReviews: requestReviews,
        requestReviewsByKeyword: requestReviewsByKeyword,
        requestReview: requestReview,
    });

    function requestKeywords(date) {
        var request = $http({
            method: "get",
            url: ("/tera/keywords/" + date ),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReviews(date) {
        var request = $http({
            method: "get",
            url: ("/tera/reviews/" + date),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReviewsByKeyword(date, keyword) {
        var request = $http({
            method: "get",
            url: ("/tera/reviewsByKeyword/" + date + "/" + keyword),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestReview(articleAutoId) {
        var request = $http({
            method: "get",
            url: ("/tera/review/" + articleAutoId),
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