using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using System;
using System.Linq;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class ReportService : IReportService
    {
        private readonly INewsArticleService _newsService;

        public ReportService(INewsArticleService newsService)
        {
            _newsService = newsService;
        }

        public ReportDto GenerateReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            var articles = _newsService.GetArticles();

            if (startDate.HasValue)
                articles = articles.Where(a => a.CreatedDate >= startDate.Value).AsQueryable();
            if (endDate.HasValue)
                articles = articles.Where(a => a.CreatedDate <= endDate.Value).AsQueryable();

            var report = new ReportDto
            {
                ByCategory = articles
                    .GroupBy(a => new { a.CategoryId, a.CategoryName })
                    .Select(g => new CategoryReport
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList(),

                ByAuthor = articles
                    .GroupBy(a => new { a.CreatedById, a.CreatedByName })
                    .Select(g => new AuthorReport
                    {
                        CreatedById = g.Key.CreatedById,
                        CreatedByName = g.Key.CreatedByName,
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList(),

                ByStatus = articles
                    .GroupBy(a => a.NewsStatus)
                    .Select(g => new StatusReport
                    {
                        Status = g.Key ? "Active" : "Inactive",
                        Count = g.Count()
                    })
                    .ToList(),

                TotalArticles = articles.Count()
            };

            return report;
        }
    }
}

