namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface INewsAuditService
    {
        IQueryable<NewsAuditDto> GetAuditInfo();
    }
    public class NewsAuditDto
    {
        public string NewsArticleId { get; set; } = null!;
        public string NewsTitle { get; set; } = null!;
        public short? UpdatedById { get; set; }          
        public string LastEditorName { get; set; } = ""; 
        public DateTime? ModifiedDate { get; set; }     
    }


}
