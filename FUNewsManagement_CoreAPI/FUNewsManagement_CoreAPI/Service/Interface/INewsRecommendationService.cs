namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface INewsRecommendationService
    {
        IQueryable<RelatedNewsDto> GetRelatedNews(string currentNewsId, int top = 3);

    }
    public class RelatedNewsDto
    {
        public string NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public short CategoryId { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
