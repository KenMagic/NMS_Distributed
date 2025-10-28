using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class NewsRecommendationService : INewsRecommendationService
    {
        private readonly IGenericRepository<NewsArticle> _newsRepo;

        public NewsRecommendationService(INewsArticleRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        public IQueryable<RelatedNewsDto> GetRelatedNews(string currentNewsId, int top = 3)
        {
            var allArticles = _newsRepo.GetAll().AsQueryable();

            // Lấy bài hiện tại
            var current = allArticles.FirstOrDefault(n => n.NewsArticleId == currentNewsId);
            if (current == null) return Enumerable.Empty<RelatedNewsDto>().AsQueryable();

            // Lấy các tag của bài hiện tại
            var currentTagIds = current.Tags.Select(t => t.TagId).ToList();

            var query = allArticles
                .Where(n => n.NewsStatus == true && n.NewsArticleId != currentNewsId)
                .Where(n => n.CategoryId == current.CategoryId
                            || n.Tags.Any(t => currentTagIds.Contains(t.TagId)))
                .OrderByDescending(n => n.CreatedDate)
                .Take(top)
                .Select(n => new RelatedNewsDto
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    CreatedDate = n.CreatedDate ?? DateTime.Now,
                    CategoryId = n.CategoryId ?? 0,
                    TagIds = n.Tags.Select(t => t.TagId).ToList()
                });

            return query;
        }
    }
}
