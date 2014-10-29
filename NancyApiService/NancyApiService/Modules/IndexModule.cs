using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Nancy;
using Nancy.Json;
using NancyApiService.DataModel;

namespace NancyApiService.Modules
{
    public class IndexModule : NancyModule
    {
        private TeraArticlesDataContext _dataContext = new TeraArticlesDataContext();

        public IndexModule() : base("/")
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/"] = _ =>
            {
                return View["Index.html"];
            };

            Get["/keywords/{beginDate}/{endDate}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                date = DateTime.Parse((string) _.endDate);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var keywordsList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Where(e => e.Keywords != null)
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
                        }));
            };

            Get["/reviews/{beginDate}/{endDate}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                date = DateTime.Parse((string)_.endDate);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var reviewList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Where(e => e.Keywords != null)
                    .Select(e => new { e.ArticleAutoId, e.Keywords, e.TargetSite, e.Author, e.Link, e.CategoryId, e.ArticleWrittenTime, e.Title, })
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
                        Title = e.Title,
                        Review = XDocument.Parse(e.Keywords).XPathSelectElement("//Document/HtmlCleanDocument").Value,
                    })
                .Where(e => e.Review.Trim().Length > 0);

                return Response.AsJson(resultReviews.OrderByDescending(e => e.ArticleWrittenTime));
            };

            Get["/reviewsByKeyword/{beginDate}/{endDate}/{keyword}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                date = DateTime.Parse((string)_.endDate);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var reviewList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Where(e => e.Keywords != null)
                    .Select(e => new { e.ArticleAutoId, e.Keywords, e.TargetSite, e.Author, e.Link, e.CategoryId, e.ArticleWrittenTime, e.Title, })
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
                        Title = e.Title,
                        Review = XDocument.Parse(e.Keywords).XPathSelectElement("//Document/HtmlCleanDocument").Value,
                    })
                .Where(e => e.Review.Trim().Length > 0);

                return Response.AsJson(resultReviews.OrderByDescending(e => e.ArticleWrittenTime));
            };

            Get["/review/{articleAutoId}"] = _ =>
            {
                var articleAutoId = (int)_.articleAutoId;
                return Response.AsJson(_dataContext.Articles.First(e => e.ArticleAutoId == articleAutoId));
            };
        }
    }
}