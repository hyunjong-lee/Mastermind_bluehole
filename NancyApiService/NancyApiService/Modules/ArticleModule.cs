using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NancyApiService.DataModel;
using Nancy.Json;

namespace NancyApiService.Modules
{
    public class ArticleModule : NancyModule
    {
        private TeraArticlesDataContext _dataContext = new TeraArticlesDataContext();

        public ArticleModule() : base("/article")
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/"] = _ =>
            {
                return View["Article.html"];
            };

            Get["/{articleId}"] = _ =>
            {
                return View["Article.html"];
            };

            Get["/api/Article/{articleId}"] = _ =>
            {
                int articleId = int.Parse(_.articleId.ToString());
                var article = _dataContext.Articles
                    .Where(e => e.ArticleAutoId == articleId)
                    .ToList()
                    .Select(e => new
                    {
                        e.ArticleAutoId,
                        e.ArticleId,
                        ArticleWrittenTime = e.ArticleWrittenTime.Value.ToString("yyyy-MM-dd HH시 mm분"),
                        e.Author,
                        e.CategoryId,
                        e.ContentHtml,
                        e.Keywords,
                        e.Link,
                        e.TargetSite,
                        e.Title,
                    })
                    .First();

                return Response.AsJson(article);
            };

            Get["/api/Comments/{articleId}"] = _ =>
            {
                int articleId = int.Parse(_.articleId.ToString());
                return Response.AsJson(_dataContext.Comments
                    .Where(e => e.ArticleAutoId == articleId)
                    .OrderBy(e => e.CommentWrittenTime)
                    .ToList()
                    .Select(e => new
                    {
                        e.ArticleAutoId,
                        e.Author,
                        e.CommentId,
                        CommentWrittenTime = e.CommentWrittenTime.Value.ToString("yyyy-MM-dd HH시 mm분"),
                        e.ContentHtml,
                        e.DislikeCount,
                        e.LikeCount,
                        e.ParentCommentId,
                    }));
            };

            Get["/api/Author/{articleId}"] = _ =>
            {
                // 저자 관련 정보 분석하여 보여주기
                return null;
            };
        }
    }
}