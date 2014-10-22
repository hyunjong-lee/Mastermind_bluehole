using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Nancy;
using Nancy.Json;
using NancyApiService.DataModel;
using NancyApiService.Helper;
using System.Xml.Linq;

namespace NancyApiService
{
    public class KeywordModule : NancyModule
    {
        private TeraArticlesDataContext _dataContext = new TeraArticlesDataContext();

        public KeywordModule() : base("/tera")
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/article"] = _ =>
            {
                return Response.AsJson(
                    _dataContext.Articles
                        .Select(e => new
                        {
                            e.ArticleAutoId,
                            e.ArticleId,
                            e.Author,
                            e.ContentHtml,
                            e.Keywords,
                        })
                        .First());
            };

            Get["/"] = _ =>
            {
                return View["Keywords.html"];
            };

            Get["/keywords/{date}"] = _ =>
            {
                var date = DateTime.Parse((string)_.date);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var keywordsList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Select(e => e.Keywords)
                    .ToList();

                var wordCount = new Dictionary<string, int>();
                foreach (var keywords in keywordsList.Where(e => !string.IsNullOrEmpty(e)))
                {
                    var xDoc = XDocument.Parse(keywords);
                    var keywordList = xDoc
                        .XPathSelectElements("//Document/Sentence")
                        .SelectMany(e => e.Value.Split(','))
                        .Where(e => e.Contains("/"))
                        .Select(e => new { Word = e.Split('/')[0], Tag = e.Split('/')[1], })
                        .Where(e => e.Word.Length > 1)
                        .Select(e => new { Word = (e.Tag[0] == 'V' ? e.Word + "다" : e.Word) + "::TAG::" + e.Word + "____" + e.Tag, })
                        .Distinct();

                    foreach (var keyword in keywordList)
                    {
                        if (!wordCount.ContainsKey(keyword.Word)) wordCount.Add(keyword.Word, 0);
                        wordCount[keyword.Word] = wordCount[keyword.Word] + 1;
                    }
                }

                return Response.AsJson(
                    wordCount
                        .OrderByDescending(e => e.Value)
                        .Select(e => new
                        {
                            WordInfo = e.Key.Split(new string[] {"::TAG::"}, StringSplitOptions.RemoveEmptyEntries),
                            Count = e.Value,
                        })
                        .Select(e => new
                        {
                            Word = e.WordInfo[0],
                            Morpheme = e.WordInfo[1],
                            Count = e.Count,
                        })
                        .Take(100));
            };

            Get["/reviews/{date}"] = _ =>
            {
                var date = DateTime.Parse((string)_.date);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var reviewList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Select(e => new { e.ArticleAutoId, e.Keywords, e.TargetSite, e.Author, e.Link, e.CategoryId, e.ArticleWrittenTime})
                    .ToList();

                var resultReviews = reviewList
                    .Select(e => new
                    {
                        ArticleId = e.ArticleAutoId,
                        Author = e.Author,
                        TargetSite = e.TargetSite.ToString(),
                        CategoryId = e.CategoryId,
                        Link = e.Link,
                        ArticleWrittenTime = e.ArticleWrittenTime.Value.ToString("yyyy-MM-dd HH시 mm분"),
                        Review = XDocument.Parse(e.Keywords).XPathSelectElement("//Document/HtmlCleanDocument").Value,
                    })
                .Where(e => e.Review.Trim().Length > 0);

                return Response.AsJson(resultReviews.OrderBy(e => e.ArticleWrittenTime).Take(100));
            };

            Get["/reviewsByKeyword/{date}/{keyword}"] = _ =>
            {
                var date = DateTime.Parse((string)_.date);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var reviewList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Select(e => new { e.ArticleAutoId, e.Keywords, e.TargetSite, e.Author, e.Link, e.CategoryId, e.ArticleWrittenTime })
                    .ToList();

                var keyword = ((string)_.keyword).Replace("____", "/");

                var resultReviews = reviewList
                    .Where(e => XDocument.Parse(e.Keywords)
                        .XPathSelectElements("//Document/Sentence")
                        .SelectMany(t => t.Value.Split(','))
                        .Any(t => t == keyword))
                    .Select(e => new
                    {
                        ArticleId = e.ArticleAutoId,
                        Author = e.Author,
                        TargetSite = e.TargetSite.ToString(),
                        CategoryId = e.CategoryId,
                        Link = e.Link,
                        ArticleWrittenTime = e.ArticleWrittenTime.Value.ToString("yyyy-MM-dd HH시 mm분"),
                        Review = XDocument.Parse(e.Keywords).XPathSelectElement("//Document/HtmlCleanDocument").Value,
                    })
                .Where(e => e.Review.Trim().Length > 0);

                return Response.AsJson(resultReviews.OrderBy(e => e.ArticleWrittenTime).Take(100));
            };

            Get["/review/{articleAutoId}"] = _ =>
            {
                var articleAutoId = (int)_.articleAutoId;
                return Response.AsJson(_dataContext.Articles.First(e => e.ArticleAutoId == articleAutoId));
            };
        }
    }
}