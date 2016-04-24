using Justin.Homepage.Models;
using System.Collections.Generic;

namespace Justin.Homepage.Repositories
{
    public interface IArticleRepository
    {
        Article Get(string id);
        string Add(Article article);
        bool Delete(string id);
        void Save(Article article);
        IEnumerable<Article> GetPage(int pageNum);
    }
}
