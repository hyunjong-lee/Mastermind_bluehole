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
        }
    }
}