﻿using System.Linq;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Configuration;

namespace TeraCrawler.TargetCrawler
{

    public class NaverCrawler : Crawler
    {
        // 네이버에 로그인하기 위한 임시 아이디를 만들었습니다
        // configuration에서 설정하십시오

        private const string cafeUrl = "http://m.cafe.naver.com/sd92";
        private const string loginReqUrl = "https://nid.naver.com/nidlogin.login";
        private const string loginFormUrl = loginReqUrl + "?&url=http://m.cafe.naver.com/sd92";
        private const string categoryUrl = "http://m.cafe.naver.com/ArticleList.nhn?search.boardtype=L&search.menuid={0}&search.questionTab=A&search.clubid=13518432&search.totalCount=201&search.page={1}";
        private const string articlUrl = "http://m.cafe.naver.com/ArticleRead.nhn?clubid=13518432&articleid={1}&page=1&boardtype=L&menuid={0}";
        private const string commentUrl = "http://m.cafe.naver.com/CommentView.nhn?search.clubid=13518432&search.articleid={0}&page={1}&sc=";
        
        // 각종 상수들

        // 카테고리Id ( menuid라는 파라미터로 전송됨 )
        // 자유게시판 : 104
        // 벨릭섭게 : 409
        // 아룬섭게 : 399
        // 카이아섭게 : 426

        // 게시글id는 articleid라는 파라미터로 전송됨

        private ChromeDriver driver = null;

        public NaverCrawler()
        {
            string id = ConfigurationManager.AppSettings["id"];
            string password = ConfigurationManager.AppSettings["password"];
            var processList = Process.GetProcessesByName("chromedriver");
            foreach( var process in processList )
            {
                process.Kill();
            }

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            var option = new ChromeOptions();
            option.AddExtension("3.7_0.crx");
            driver = new ChromeDriver(chromeDriverService, option );

            while (driver.WindowHandles.Count < 2)
            {
                Thread.Sleep(100);
            }

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            WebDriverWait _wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));

            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 10));

            _wait.Until(d => d.FindElement(By.Id("user_email")));
            driver.FindElementById("user_email").SendKeys(String.Format("{0}@naver.com\n", id));
            _wait.Until(d => d.FindElement(By.Id("user_password")));
            driver.FindElementById("user_password").SendKeys(password);
            driver.FindElementById("loginbtn").Click();

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.Url = loginFormUrl;
            driver.FindElementByCssSelector("#id").SendKeys(id);
            driver.FindElementByCssSelector("#pw").SendKeys(password);

            driver.FindElementByCssSelector("input.int_jogin").Click();

            cookieContainer = new CookieContainer();
            ReadOnlyCollection<OpenQA.Selenium.Cookie> cookieCollections = null; 

            bool sessionFound = false;
            while (!sessionFound)
            {
                cookieCollections = driver.Manage().Cookies.AllCookies;
                foreach( var cookie in cookieCollections )
                {
                    if ( cookie.Name == "JSESSIONID" )
                    {
                        sessionFound = true;
                        break;
                    }
                }
            }
            foreach( var cookie in cookieCollections )
            {
                cookieContainer.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
            }

            driver.Quit();
        }

        public override void ParseArticlePage(Article article)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();

            htmlDoc.LoadHtml(article.RawHtml);

            // 본문이 삭제된 경우
            if (htmlDoc.DocumentNode.ChildNodes.Count == 1) return;

            article.Author = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ct\"]/div[1]/p/span/em/a[1]")[0].InnerHtml.Trim();

            var writtenTimeNode = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ct\"]/div[1]/p/span/span[1]/text()");
            foreach( var node in writtenTimeNode )
            {
                var text = node.InnerHtml.Trim();
                if (text.Length > 0)
                {
                    article.ArticleWrittenTime = DateTime.Parse(text);
                    break;
                }
            }

            article.Title = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ct\"]/div[1]/h2")[0].InnerHtml.Trim();

            article.ContentHtml = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"postContent\"]")[0].InnerHtml.Trim();          
        }

        public override IEnumerable<Article> ParsePagingPage(string rawHtml)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            var articleNodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"article_lst_section\"]/ul[2]/li");
            if (articleNodes == null)
            {
                yield break;
            }

            foreach (var articleNode in articleNodes)
            {
                var link = String.Format("http://m.cafe.naver.com/{0}",articleNode.SelectSingleNode("a").Attributes["href"].Value);

                var match = new Regex("articleid=(?<articleid>[0-9]+)").Match(link);

                int articleId = 0;
                if (!int.TryParse(match.Groups["articleid"].Value, out articleId))
                {
                    continue;
                }               

                var article = new Article
                {
                    Game = (int)Games.tera,
                    TargetSite = (int)TargetSites.naver,
                    CategoryId = CategoryId,
                    ArticleId = articleId,
                    Link = link,
                };

                yield return article;
            }
        }

        protected override int PagingSize()
        {
            return 20;
        }

        protected override string MakePagingPageAddress(int pageNo)
        {
            return string.Format(categoryUrl, CategoryId, pageNo);
        }

        protected override string MakeArticlePageAddress(int articleId)
        {
            return string.Format(articlUrl, CategoryId, articleId);
        }

        protected override IEnumerable<string> MakeCommentPageAddresses(Article article)
        {
            List<String> listCommentPageAddress = new List<string>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(article.RawHtml);
            var node = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"ct\"]/div/ul/li/a/span/em");
            {
                int commentCount = Int32.Parse(node.InnerText);
                // 네이버는 100개가 한 페이지
                int pageCount = commentCount / 100 + 1;
                for ( int i = 1 ; i < pageCount + 1 ; ++i)
                listCommentPageAddress.Add(String.Format(commentUrl, article.ArticleId, i));
            }            
            return listCommentPageAddress;
        }

        public override void ParseCommentPage(string commentPageRawHtml, ref IList<Comment> comments)
        {
            var comment = new Comment();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(commentPageRawHtml);
            if (htmlDoc.DocumentNode.SelectNodes("//*[@class=\"lst_wp\"]") == null)
                return;

            foreach (var node in htmlDoc.DocumentNode.SelectNodes("//*[@class=\"lst_wp\"]"))
            {
                // node로부터.
                // li[1]/div/strong/a[1]       이름
                // li[1]/div/strong/em         시간
                // li[1]/div/div[2]/span       내용
                // 댓글의 댓글은 re
                // 댓글의 댓글의 댓글부터는 re2
                if (node.SelectNodes("strong/a") != null)
                {
                    comment.Author = node.SelectSingleNode("strong/a").InnerText;
                    comment.CommentWrittenTime = DateTime.Parse(node.SelectSingleNode("strong/em").InnerText);
                    comment.ContentHtml = node.SelectSingleNode("div[@class=\"clst_cont\"]/span").InnerText;
                    
                    comments.Add(comment);
                }
            }
        }

        protected override IList<Comment> CrawlComments(Article article)
        {
            IList<Comment> comments = new List<Comment>();

            var commentPages = MakeCommentPageAddresses(article);
            foreach (var commentPage in commentPages)
            {
                ParseCommentPage(commentPage.CrawlIt(encoding, headerCollection, cookieContainer), ref comments);
            }

            foreach (var comment in comments)
            {
                comment.ArticleAutoId = article.ArticleAutoId;
            }

            return comments;
        }
    }
}
