var keywordApp = angular.module('keywordApp', ['angles']);

keywordApp.controller('KeywordController', function ($scope, $log, $sce, KeywordService) {

    $scope.inited = false;

    $scope.Keyword = "";
    $scope.Morpheme = "";

    $scope.WordDistribution = [];
    $scope.RelatedWords = [];
    $scope.RelatedArticles = [];

    $scope.wordFrequencyData = {
        labels: [],
        datasets: []
    };

    $scope.refreshData = function (morpheme, beginDate, endDate) {

        if (morpheme == null)
            morpheme = $scope.Morpheme;
        if (beginDate == null)
            beginDate = $('#beginDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");
        if (endDate == null)
            endDate = $('#endDate').data('DateTimePicker').getDate().format("YYYY-MM-DD");

        console.log(morpheme);
        console.log(beginDate);
        console.log(endDate);

        $scope.getWordDistribution(morpheme, beginDate, endDate);
        $scope.getRelatedWords(morpheme, beginDate, endDate);
        $scope.getRelatedArticles(morpheme, beginDate, endDate);

    };

    $scope.getWordDistribution = function (morpheme, beginDate, endDate) {

        KeywordService
            .requestWordDistribution(morpheme, beginDate, endDate)
            .then(function (wordDistribution) {
                $scope.wordFrequencyData.labels = wordDistribution.Labels;
                $scope.wordFrequencyData.datasets = [{
                    label: "My First dataset",
                    fillColor: "rgba(220,220,220,0.2)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#fff",
                    pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(220,220,220,1)",
                    data: wordDistribution.Data,
                }];

                if ($scope.inited == false) {
                $scope.inited = true;

                var params = (location.pathname.match(/\/keyword\/(.*)/)[1]).split("/");
                $('#beginDate').data('DateTimePicker').setDate(params[1]);
                $('#endDate').data('DateTimePicker').setDate(params[2]);
            }
        });
    };

    $scope.getRelatedWords = function (morpheme, beginDate, endDate) {

        KeywordService
            .requestRelatedWords(morpheme, beginDate, endDate)
            .then(function (words) {
                // 연관 단어 보여주기
                $scope.RelatedWords = words;
            });
    };

    $scope.getRelatedArticles = function (morpheme, beginDate, endDate) {

        KeywordService
            .requestRelatedArticles(morpheme, beginDate, endDate)
            .then(function (articles) {
                // 연관 문서 보여주기
                $scope.RelatedArticles = articles;
            });
    };

    $scope.renderHtml = function (htmlCode) {
        return $sce.trustAsHtml(htmlCode);
    };

    $scope.MorphemeToKeyword = function (morpheme) {
        var strArr = morpheme.split("____");

        if (strArr[1][0] == 'V') return strArr[0] + "다";
        return strArr[0];
    }

    var params = (location.pathname.match(/\/keyword\/(.*)/)[1]).split("/");
    console.log(params);

    $scope.Morpheme = decodeURIComponent(params[0]);
    $scope.Keyword = $scope.MorphemeToKeyword($scope.Morpheme);

    $scope.refreshData($scope.Morpheme, params[1], params[2]);
});

keywordApp.service('KeywordService', function ($http, $q) {

    return ({
        requestWordDistribution: requestWordDistribution,
        requestRelatedWords: requestRelatedWords,
        requestRelatedArticles: requestRelatedArticles,
    });

    function requestWordDistribution(morpheme, beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/keyword/wordDistribution/" + morpheme + "/" + beginDate + "/" + endDate),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestRelatedWords(morpheme, beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/keyword/relatedWords/" + morpheme + "/" + beginDate + "/" + endDate),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestRelatedArticles(morpheme, beginDate, endDate) {
        var request = $http({
            method: "get",
            url: ("/keyword/relatedArticles/" + morpheme + "/" + beginDate + "/" + endDate),
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
