using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NancyApiService.DataModel;
using Nancy.Json;
using NancyApiService.Helper;

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

            Get["/{articleId}/{beginDate}/{endDate}"] = _ =>
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
                int articleId = int.Parse(_.articleId.ToString());
                var article = _dataContext.Articles.First(e => e.ArticleAutoId == articleId);

                var author = article.Author;
                var authorWrittenList = _dataContext.Articles
                    .Where(e => e.Author == author)
                    .Select(e => new
                    {
                        e.ArticleWrittenTime,
                    })
                    .ToList()
                    .Select(e => e.ArticleWrittenTime.Value.DayOfWeek)
                    .GroupBy(e => e)
                    .OrderBy(e => e.Key)
                    .Select(e => new
                    {
                        DayOfWeek = e.Key,
                        Count = e.Count(),
                    })
                    .ToDictionary(e => e.DayOfWeek, e => e.Count);

                var dateOfWeekCountDic = new Dictionary<DayOfWeek, int>();
                dateOfWeekCountDic.Add(DayOfWeek.Sunday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Monday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Tuesday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Wednesday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Thursday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Friday, 0);
                dateOfWeekCountDic.Add(DayOfWeek.Saturday, 0);
                foreach (var writtenInfo in authorWrittenList)
                {
                    dateOfWeekCountDic[writtenInfo.Key] = writtenInfo.Value;
                }
                
                return Response.AsJson(new {
                    TotalCount = authorWrittenList.Sum(e => e.Value),
                    Labels = new List<string> { "일", "월", "화", "수", "목", "금", "토"},
                    Data = dateOfWeekCountDic.Select(e => e.Value),
                });
            };
        }
    }
}