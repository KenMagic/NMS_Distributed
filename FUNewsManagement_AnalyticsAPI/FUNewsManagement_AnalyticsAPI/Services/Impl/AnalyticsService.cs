using FUNewsManagement_AnalysticsAPI.Models;
using FUNewsManagement_AnalyticsAPI.Models;
using FUNewsManagement_AnalyticsAPI.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using OfficeOpenXml;

namespace FUNewsManagement_AnalyticsAPI.Services.Impl
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly FunewsManagementContext _db;
        public AnalyticsService(FunewsManagementContext db)
        {
            _db = db;
        }

        public DashboardDto GetDashboard()
        {
            // By Category
            var byCategory = _db.Categories
                .Select(c => new CategoryCountDto
                {
                    CategoryName = c.CategoryName,
                    Count = c.NewsArticles.Count(na => na.NewsStatus == true)
                }).ToList();

            // By Status
            var published = _db.NewsArticles.Count(n => n.NewsStatus == true);
            var notPublished = _db.NewsArticles.Count(n => n.NewsStatus != true);

            var byStatus = new List<StatusCountDto>
            {
                new StatusCountDto { Status = "Published", Count = published },
                new StatusCountDto { Status = "NotPublished", Count = notPublished }
            };

            return new DashboardDto { ByCategory = byCategory, ByStatus = byStatus };
        }

        public IEnumerable<ArticleListItemDto> GetTrending(int top = 5)
        {
            // Dựa theo NewsView (nếu đã có)
            var trendingQuery = _db.NewsViews
                .GroupBy(v => v.NewsArticleId)
                .Select(g => new { NewsArticleId = g.Key, ViewCount = g.Count() })
                .OrderByDescending(g => g.ViewCount)
                .Take(top)
                .ToList();

            var articleIds = trendingQuery.Select(x => x.NewsArticleId).ToList();

            var articles = _db.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.Tags)
                .Include(n => n.CreatedBy)
                .Where(n => articleIds.Contains(n.NewsArticleId))
                .ToList();

            return articles.Select(n => new ArticleListItemDto
            {
                NewsArticleId = n.NewsArticleId,
                Title = n.NewsTitle,
                Headline = n.Headline,
                CategoryId = n.CategoryId,
                CategoryName = n.Category?.CategoryName,
                AuthorId = n.CreatedById,
                AuthorName = n.CreatedBy?.AccountName,
                NewsStatus = n.NewsStatus,
                CreatedDate = n.CreatedDate,
                ModifiedDate = n.ModifiedDate,
                Tags = n.Tags.Select(t => t.TagName ?? "").ToList()
            }).ToList();
        }

        public IEnumerable<ArticleListItemDto> Recommend(string newsArticleId, int take = 5)
        {
            var seed = _db.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);

            if (seed == null) return Enumerable.Empty<ArticleListItemDto>();

            var tagIds = seed.Tags.Select(t => t.TagId).ToList();
            short? categoryId = seed.CategoryId;

            var q = _db.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.Tags)
                .Include(n => n.CreatedBy)
                .Where(n => n.NewsArticleId != newsArticleId &&
                            (n.CategoryId == categoryId || n.Tags.Any(t => tagIds.Contains(t.TagId))))
                .OrderByDescending(n => n.ModifiedDate ?? n.CreatedDate)
                .Take(take)
                .ToList();

            return q.Select(n => new ArticleListItemDto
            {
                NewsArticleId = n.NewsArticleId,
                Title = n.NewsTitle,
                Headline = n.Headline,
                CategoryId = n.CategoryId,
                CategoryName = n.Category?.CategoryName,
                AuthorId = n.CreatedById,
                AuthorName = n.CreatedBy?.AccountName,
                NewsStatus = n.NewsStatus,
                CreatedDate = n.CreatedDate,
                ModifiedDate = n.ModifiedDate,
                Tags = n.Tags.Select(t => t.TagName ?? "").ToList()
            }).ToList();
        }

        public IEnumerable<ArticleListItemDto> Filter(short? categoryId, short? authorId, DateTime? from, DateTime? to, bool? status)
        {
            var q = _db.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.Tags)
                .Include(n => n.CreatedBy)
                .AsQueryable();

            if (categoryId.HasValue) q = q.Where(n => n.CategoryId == categoryId.Value);
            if (authorId.HasValue) q = q.Where(n => n.CreatedById == authorId.Value);
            if (status.HasValue) q = q.Where(n => n.NewsStatus == status.Value);
            if (from.HasValue) q = q.Where(n => (n.CreatedDate ?? n.ModifiedDate) >= from.Value);
            if (to.HasValue) q = q.Where(n => (n.CreatedDate ?? n.ModifiedDate) <= to.Value);

            var list = q
                .OrderByDescending(n => n.ModifiedDate ?? n.CreatedDate)
                .ToList();

            return list.Select(n => new ArticleListItemDto
            {
                NewsArticleId = n.NewsArticleId,
                Title = n.NewsTitle,
                Headline = n.Headline,
                CategoryId = n.CategoryId,
                CategoryName = n.Category?.CategoryName,
                AuthorId = n.CreatedById,
                AuthorName = n.CreatedBy?.AccountName,
                NewsStatus = n.NewsStatus,
                CreatedDate = n.CreatedDate,
                ModifiedDate = n.ModifiedDate,
                Tags = n.Tags.Select(t => t.TagName ?? "").ToList()
            }).ToList();
        }

        public byte[] ExportToExcel(short? categoryId, short? authorId, DateTime? from, DateTime? to, bool? status)
        {
            var items = Filter(categoryId, authorId, from, to, status).ToList();
            using var p = new ExcelPackage();
            var ws = p.Workbook.Worksheets.Add("Analytics");

            // Header
            ws.Cells[1, 1].Value = "ArticleId";
            ws.Cells[1, 2].Value = "Title";
            ws.Cells[1, 3].Value = "Headline";
            ws.Cells[1, 4].Value = "Category";
            ws.Cells[1, 5].Value = "Author";
            ws.Cells[1, 6].Value = "Status";
            ws.Cells[1, 7].Value = "CreatedDate";
            ws.Cells[1, 8].Value = "ModifiedDate";
            ws.Cells[1, 9].Value = "Tags";

            var row = 2;
            foreach (var it in items)
            {
                ws.Cells[row, 1].Value = it.NewsArticleId;
                ws.Cells[row, 2].Value = it.Title;
                ws.Cells[row, 3].Value = it.Headline;
                ws.Cells[row, 4].Value = it.CategoryName;
                ws.Cells[row, 5].Value = it.AuthorName;
                ws.Cells[row, 6].Value = it.NewsStatus.HasValue ? (it.NewsStatus.Value ? "Published" : "NotPublished") : "";
                ws.Cells[row, 7].Value = it.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cells[row, 8].Value = it.ModifiedDate?.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cells[row, 9].Value = string.Join(", ", it.Tags ?? Enumerable.Empty<string>());
                row++;
            }

            return p.GetAsByteArray();
        }

    }
}
