var keywordApp = angular.module('keywordApp', ['ui.bootstrap']);

keywordApp.controller('KeywordController', function ($scope, $modal, $log, KeywordService) {

    $scope.Keyword = "";
    $scope.Morpheme = "";

    $scope.refreshData = function (morpheme) {

    };

    //$scope.getReviewsByKeyword = function (keyword) {

    //    var beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
    //    var endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");

    //    console.log(beginDate);
    //    console.log(endDate);
    //    console.log(keyword);

    //    KeywordService
    //        .requestReviewsByKeyword(beginDate, endDate, keyword)
    //        .then(function (reviews) {
    //            $scope.Reviews = reviews;
    //        });
    //};

    $scope.renderHtml = function (htmlCode) {
        return $sce.trustAsHtml(htmlCode);
    };

    var morpheme = decodeURIComponent(location.pathname.match(/\/keyword\/(.*)/)[1]);
    console.log(morpheme);

    $scope.Keyword = morpheme;
    $scope.Morpheme = morpheme;
    $scope.refreshData(morpheme);
});

keywordApp.service('KeywordService', function ($http, $q) {

    return ({
    });

    function requestKeywords(beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/keyword/" + beginDate + "/" + endDate),
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
