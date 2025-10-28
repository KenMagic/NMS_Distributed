using FUNewsManagement_AnalysticsAPI.Models;

namespace FUNewsManagement_AnalyticsAPI.Services.Interface
{
    public interface IAnalyticsService
    {
        DashboardDto GetDashboard();
        IEnumerable<ArticleListItemDto> GetTrending(int top = 5);
        IEnumerable<ArticleListItemDto> Recommend(string newsArticleId, int take = 5);
        IEnumerable<ArticleListItemDto> Filter(short? categoryId, short? authorId, DateTime? from, DateTime? to, bool? status);
        byte[] ExportToExcel(short? categoryId, short? authorId, DateTime? from, DateTime? to, bool? status);
    }
}

