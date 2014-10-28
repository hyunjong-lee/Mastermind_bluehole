var articleApp = angular.module('articleApp', []);

articleApp.controller('ArticleController', function ($scope, $log, $sce, ArticleService) {

    $scope.Article = [];
    $scope.Comments = {};
    $scope.Author = [];

    $scope.refreshData = function (articleId) {

        $scope.getArticle(articleId);
        $scope.getComments(articleId);
        $scope.getAuthor(articleId);

    };

    $scope.getArticle = function (articleId) {

        console.log(articleId);

        ArticleService
            .requestArticle(articleId)
            .then(function (article) {
                $scope.Article = article;
            });
    };

    $scope.getComments = function (articleId) {

        console.log(articleId);

        ArticleService
            .requestComments(articleId)
            .then(function (comments) {
                $scope.Comments = comments;
            });
    };

    $scope.getAuthor = function (articleId) {

        console.log(articleId);

        ArticleService
            .requestAuthor(articleId)
            .then(function (author) {
                $scope.Author = author;
            });
    };

    $scope.renderHtml = function (htmlCode) {
        return $sce.trustAsHtml(htmlCode);
    };

    var articleId = location.pathname.match(/\/article\/(.*)/)[1];
    console.log(articleId);

    $scope.refreshData(articleId);

});

articleApp.service('ArticleService', function ($http, $q) {

    return ({
        requestArticle: requestArticle,
        requestComments: requestComments,
        requestAuthor: requestAuthor,
    });

    function requestArticle(articleId) {
        var request = $http({
            method: "get",
            url: ("/article/api/Article/" + articleId),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestComments(articleId) {
        var request = $http({
            method: "get",
            url: ("/article/api/Comments/" + articleId),
        });

        return (request.then(handleSuccess, handleError));
    }

    function requestAuthor(articleId) {
        var request = $http({
            method: "get",
            url: ("/article/api/Author/" + articleId),
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
