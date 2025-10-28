using System;
using FUNewsManagement_CoreAPI.Models;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface IReportService
    {
        ReportDto GenerateReport(DateTime? startDate = null, DateTime? endDate = null);
    }

    public class ReportDto
    {
        public List<CategoryReport> ByCategory { get; set; } = new List<CategoryReport>();
        public List<AuthorReport> ByAuthor { get; set; } = new List<AuthorReport>();
        public List<StatusReport> ByStatus { get; set; } = new List<StatusReport>();
        public int TotalArticles { get; set; }
    }

    public class CategoryReport
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AuthorReport
    {
        public short CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusReport
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
