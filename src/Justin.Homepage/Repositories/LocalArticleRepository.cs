using Justin.Homepage.Models;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Justin.Homepage.Repositories
{
    public class LocalArticleRepository : IArticleRepository
    {
        private string m_articleRootPath;
        private int m_loadNum = 8;

        private const string HTML_DIR = "html";
        private const string DB_FILE = "db.txt";
        
        private static IList<Article> s_allArticles;
        private static bool s_articleLoaded = false;
        private static readonly object s_lock = new object();

        private void LoadArticlesWithLock()
        {
            lock(s_lock)
            {
                if(!s_articleLoaded)
                {
                    s_allArticles = LoadAllArticles();
                    s_articleLoaded = true;
                }
            }
        }
        private IList<Article> LoadAllArticles()
        {
            List<Article> articles = new List<Article>();

            string db_file_path = Path.Combine(m_articleRootPath, DB_FILE);
            using (TextReader reader = new StreamReader(File.OpenRead(db_file_path)))
            {
                string line = null;
                while (!string.IsNullOrEmpty((line = reader.ReadLine())))
                {
                    var op = JsonConvert.DeserializeObject<DbOp>(line);
                    switch (op.Op)
                    {
                        case DbOpType.Add:
                            articles.Add(op.Article);
                            break;
                        case DbOpType.Delete:
                            articles.Remove(new Article { Id = op.ArticleId });
                            break;
                        default:
                            break;
                    }
                }
            }

            return articles.OrderByDescending(a => a.Time).ToList();
        }
        private void AddArticleLock(Article article)
        {
            lock (s_allArticles)
            {
                if (s_allArticles.IndexOf(article) >= 0)
                    throw new Exception("已经存在相同id的文章");

                s_allArticles.Add(article);
            }
        }
        private void RemoveArticleLock(Article article)
        {
            lock (s_allArticles)
            {
                int idx = s_allArticles.IndexOf(article);
                if (idx >= 0)
                    s_allArticles.RemoveAt(idx);
            }
        }
        private void SaveArticleLock(Article article)
        {
            int idx = s_allArticles.IndexOf(article);
            if(idx >= 0)
            {
                var old = s_allArticles[idx];
                article.Html = null;
                article.Time = old.Time;
                s_allArticles[idx] = article;
            }
        }
        private void AddHtmlToFile(string id, string html)
        {
            File.WriteAllText(HtmlPath(id), html, Encoding.UTF8);
        }
        private void DeleteHtmlFile(string id)
        {
            string file_path = HtmlPath(id);

            if (File.Exists(file_path))
            {
                File.Delete(file_path);
            }
        }
        private string ReadHtml(string id)
        {
            string html_path = HtmlPath(id);
            return File.ReadAllText(html_path, Encoding.UTF8);
        }
        private string AddDbOp(Article article, DbOpType opType)
        {
            string db_file_path = DbFilePath;
            article.Html = null;
            string opId = Guid.NewGuid().ToString();

            var op = new DbOp
            {
                Id = opId,
                Op = opType,
            };

            switch (opType)
            {
                case DbOpType.Add:
                    op.Article = article;
                    break;
                case DbOpType.Delete:
                    op.ArticleId = article.Id;
                    break;
                default:
                    break;
            }

            using (TextWriter writer =
                new StreamWriter(new FileStream(db_file_path, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine(JsonConvert.SerializeObject(op));
            }

            return op.Id;
        }

        public LocalArticleRepository(IHostingEnvironment env, IConfiguration config)
        {
            m_loadNum = int.Parse(config["loadNumOfArticle"]);
            m_articleRootPath = Path.Combine(env.WebRootPath, "articles");

            if(!s_articleLoaded)
            {
                LoadArticlesWithLock();
            }
        }
        
        public string DbFilePath
        {
            get
            {
                return Path.Combine(m_articleRootPath, DB_FILE);
            }
        }
        public string HtmlPath(string articleId)
        {
            return Path.Combine(m_articleRootPath, HTML_DIR, articleId + ".html");
        }

        public string Add(Article article)
        {
            article.Time = DateTime.Now;

            try
            {
                AddHtmlToFile(article.Id, article.Html);
                AddDbOp(article, DbOpType.Add);
                AddArticleLock(article);
            }
            catch
            {
                RemoveArticleLock(article);
                DeleteHtmlFile(article.Id);
            }

            return article.Id;
        }
        public bool Delete(string id)
        {
            var article = s_allArticles.FirstOrDefault(a => a.Id.Equals(id));

            if (article == null)
                return true;

            try
            {
                RemoveArticleLock(article);
                AddDbOp(article, DbOpType.Delete);
                DeleteHtmlFile(id);

                return true;
            }
            catch
            {
                AddArticleLock(article);
                return false;
            }
        }
        public Article Get(string id)
        {
            Article article = s_allArticles.FirstOrDefault(a => a.Id.Equals(id));
            if(article != null)
            {
                article.Html = ReadHtml(id);
            }

            return article;
        }
        public void Save(Article article)
        {
            AddHtmlToFile(article.Id, article.Html);
            AddDbOp(new Article { Id = article.Id }, DbOpType.Delete);
            AddDbOp(article, DbOpType.Add);
            SaveArticleLock(article);
        }
        public IEnumerable<Article> GetPage(int pageNum)
        {
            return s_allArticles.Skip((pageNum - 1) * m_loadNum).Take(m_loadNum);
        }

        public class DbOp
        {
            public string Id { get; set; }
            public DbOpType Op { get; set; }
            public Article Article { get; set; }
            public string ArticleId { get; set; }
        }
        public enum DbOpType
        {
            Add,
            Delete,
        }
    }
}
