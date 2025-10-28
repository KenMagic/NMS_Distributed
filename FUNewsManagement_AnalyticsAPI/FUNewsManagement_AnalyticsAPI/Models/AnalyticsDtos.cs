using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement_AnalysticsAPI.Models
{
    public class DashboardDto
    {
        public IEnumerable<CategoryCountDto> ByCategory { get; set; } = new List<CategoryCountDto>();
        public IEnumerable<StatusCountDto> ByStatus { get; set; } = new List<StatusCountDto>();
    }

    public class CategoryCountDto
    {
        public string CategoryName { get; set; } = "";
        public int Count { get; set; }
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class ArticleListItemDto
    {
        [Key]
        public string NewsArticleId { get; set; } = "";
        public string? Title { get; set; }
        public string? Headline { get; set; }
        public string? CategoryName { get; set; }
        public short? CategoryId { get; set; }
        public short? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public bool? NewsStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        int Views {get; set;}
    }
}
