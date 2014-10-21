var keywordApp = angular.module('keywordApp', []);

keywordApp.controller('KeywordController', function ($scope, KeywordService) {

    $scope.Keywords = [];
    $scope.Reviews = [];

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

    $scope.refreshData("2014-10-01");
});

keywordApp.service('KeywordService', function($http, $q) {

    return ({
        requestKeywords: requestKeywords,
        requestReviews: requestReviews,
        requestReviewsByKeyword: requestReviewsByKeyword,
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