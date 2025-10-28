using FUNewsManagement_CoreAPI.Models;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface ITagService
    {
        IEnumerable<Tag> GetTags(string? tagName = null);
        Tag GetTagById(int id);
        Tag CreateTag(Tag tag);
        Tag UpdateTag(int id, Tag tag);
        void DeleteTag(int id);
        IEnumerable<NewsArticleByTagDto> GetArticlesByTag(int tagId);
    }
    public class NewsArticleByTagDto
    {
        public string NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }

        // Category info
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public bool NewsStatus { get; set; }

        // Author info
        public short CreatedById { get; set; }
        public string CreatedByName { get; set; } = string.Empty;

        public short? UpdatedById { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // List of tags attached to this article
        public List<TagDto> Tags { get; set; } = new();
    }

    public class TagDto
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

}
