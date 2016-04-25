using Justin.Homepage.Models;
using Justin.Homepage.Repositories;
using Justin.Homepage.Util;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Justin.Homepage.Controllers
{
    public class ArticleController : BaseController
    {
        IArticleRepository _article;
        IConfiguration _config;

        public ArticleController(IArticleRepository article, IConfiguration config)
        {
            _article = article;
            _config = config;
        }

        [HttpGet]
        public IActionResult Index(string id)
        {
            var model = _article.Get(id);

            if (model == null)
                return NotFound();

            ViewBag.IsMobile = new Regex("iPhone|iPad|iPod|Android|IEMobile", RegexOptions.IgnoreCase)
                            .IsMatch(Request.Headers["User-Agent"].ToString());

            return View(model);
        }

        [HttpGet]
        public IActionResult New()
        {
            var model = Article.Empty;
            model.Id = DateTime.Now.ToString("yyyyMMddHHmmss");

            return View(model);
        }
        
        [HttpPost]
        public IActionResult New(Article article, string pwd)
        {
            if (!ValidPwd(pwd) || !ValidArticle(article))
                return View(article);

            string id = _article.Add(article);

            if(!string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", new { id = id });
            }
            else
            {
                return View(article);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var model = _article.Get(id);

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Article article, string pwd)
        {
            if (!ValidPwd(pwd) || !ValidArticle(article))
                return View(article);

            _article.Save(article);

            return RedirectToAction("Index", new { id = article.Id });
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var article = _article.Get(id);

            if (article == null)
                return NotFound();

            return View(article);
        }

        [HttpPost]
        public IActionResult Delete(string id, string pwd)
        {
            var article = _article.Get(id);
            if (article == null)
                return NotFound();

            if (!ValidPwd(pwd))
                return View(article);

            if (_article.Delete(id))
            {
                return Redirect("/");
            }
            else
            {
                return Error();
            }
        }

        [HttpGet]
        public IActionResult Page(int id)
        {
            var model = _article.GetPage(id);

            if (!model.Any())
                return Content(string.Empty, "text/html");

            return PartialView(model);
        }

        [HttpPost]
        public IActionResult CKUploadImage(string id, IFormFile upload, string CKEditorFuncNum)
        {
            string virtualPath = "articles/images/" + id;
            string img_dir = Path.Combine(Request.HttpContext.RequestServices
                    .GetService<IHostingEnvironment>().WebRootPath, virtualPath);
            
            if(!Directory.Exists(img_dir))
            {
                Directory.CreateDirectory(img_dir);
            }

            string img_id = Guid.NewGuid().ToString();
            string target = Path.Combine(img_dir, img_id);

            using (Stream file = new FileStream(target, 
                FileMode.OpenOrCreate, FileAccess.Write))
            {
                upload.OpenReadStream().CopyTo(file, 0x1000); 
            }

            string imageUrl = Url.Link("article-image", new { articleId = id, id = img_id });
            string result = string.Format(
                @"<script type ='text/javascript'>
                    window.parent.CKEDITOR.tools.callFunction({0},'{1}','');
                  </script>", CKEditorFuncNum, imageUrl);

            return Content(result, "text/html");
        }

        [HttpGet]
        public IActionResult Image(string articleId, string id)
        {
            return File("articles/images/" + articleId + "/" + id, "image/jpeg");
        }
        
        private bool ValidPwd(string pwd)
        {
            string articleEditPwd = _config["articleEditPwd"];

            return !string.IsNullOrEmpty(pwd) &&
                Md5Helper.Md5(pwd).Equals(articleEditPwd);
        }
        private bool ValidArticle(Article article)
        {
            return !string.IsNullOrEmpty(article.Title) &&
                !string.IsNullOrEmpty(article.Abstract) &&
                !string.IsNullOrEmpty(article.ImageUrl) &&
                !string.IsNullOrEmpty(article.Html);
        }
    }
}
