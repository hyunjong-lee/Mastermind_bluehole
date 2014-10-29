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

        public KeywordModule() : base("/keyword")
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/"] = _ =>
            {
                return View["Keyword.html"];
            };

            Get["/{morpheme}"] = _ =>
            {
                var morpheme = _.morpheme.ToString();
                return View["Keyword.html"];
            };

            Get["/{morpheme}/{beginDate}/{endDate}"] = _ =>
            {
                return View["Keyword.html"];
            };

            Get["/wordDistribution/{morpheme}/{beginDate}/{endDate}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = (new DateTime(date.Year, date.Month, date.Day)).ToInt();
                date = DateTime.Parse((string)_.endDate);
                var endDate = (new DateTime(date.Year, date.Month, date.Day)).ToInt().AddDays(1);
                var morpheme = ((string)_.morpheme).Replace("____", "/");

                var indexInfo = _dataContext.MorphemeIndexers.FirstOrDefault(e => e.Morpheme == morpheme);
                var relatedDocumentList = indexInfo.DocumentSet.Split(',')
                    .Select(e => e.Split('|'))
                    .Select(e => new {ArticleAutoId = int.Parse(e[0]), Date = int.Parse(e[1]),})
                    .Where(e => beginDate <= e.Date)
                    .Where(e => e.Date < endDate)
                    .ToList();

                var countDic = relatedDocumentList
                    .GroupBy(e => e.Date)
                    .Select(e => new {Date = e.Key, Count = e.Count(),})
                    .ToDictionary(e => e.Date, e => e.Count);

                var resultDic = beginDate.DateRange(endDate.AddDays(-1))
                    .ToDictionary(
                        e => e,
                        e => countDic.ContainsKey(e) ? countDic[e] : 0);

                return Response.AsJson(new
                {
                    TotalCount = resultDic.Sum(e => e.Value),
                    Labels = resultDic.Select(e => e.Key.ToDateString()),
                    Data = resultDic.Select(e => e.Value),
                });

                return null;
            };

            Get["/relatedWords/{morpheme}/{beginDate}/{endDate}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = (new DateTime(date.Year, date.Month, date.Day)).ToInt();
                date = DateTime.Parse((string)_.endDate);
                var endDate = (new DateTime(date.Year, date.Month, date.Day)).ToInt().AddDays(1);
                var morpheme = ((string)_.morpheme).Replace("____", "/");

                var indexInfo = _dataContext.MorphemeIndexers.FirstOrDefault(e => e.Morpheme == morpheme);
                var relatedDocumentList = indexInfo.DocumentSet.Split(',')
                    .Select(e => e.Split('|'))
                    .Select(e => new { ArticleAutoId = int.Parse(e[0]), Date = int.Parse(e[1]), })
                    .Where(e => beginDate <= e.Date)
                    .Where(e => e.Date < endDate)
                    .ToList();

                return null;
            };

            Get["/relatedArticles/{morpheme}/{beginDate}/{endDate}"] = _ =>
            {
                var date = DateTime.Parse((string)_.beginDate);
                var beginDate = new DateTime(date.Year, date.Month, date.Day);
                date = DateTime.Parse((string)_.endDate);
                var endDate = new DateTime(date.Year, date.Month, date.Day).AddDays(1);
                var morpheme = ((string)_.morpheme).Replace("____", "/");
                
                var reviewList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime < endDate)
                    .Where(e => e.Keywords != null)
                    .Select(e => new { e.ArticleAutoId, e.Keywords, e.TargetSite, e.Author, e.Link, e.CategoryId, e.ArticleWrittenTime, e.Title, })
                    .ToList();

                var resultReviews = reviewList
                    .Where(e => XDocument.Parse(e.Keywords)
                        .XPathSelectElements("//Document/Sentence")
                        .SelectMany(t => t.Value.Split(','))
                        .Any(t => t == morpheme))
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
        }
    }
}