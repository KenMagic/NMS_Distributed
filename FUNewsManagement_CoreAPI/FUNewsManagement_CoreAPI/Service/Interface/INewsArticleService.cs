using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Service.Impl;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface INewsArticleService
    {
        IQueryable<NewsArticleDto> GetArticles(); 
        NewsArticle CreateArticle(NewsArticle article, short currentUserId, List<int>? tagIds);
        NewsArticle UpdateArticle(string id, NewsArticle article, short currentUserId, List<int>? tagIds);
        void DeleteArticle(string id);
        NewsArticle DuplicateArticle(string id, short currentUserId);

    }
}
