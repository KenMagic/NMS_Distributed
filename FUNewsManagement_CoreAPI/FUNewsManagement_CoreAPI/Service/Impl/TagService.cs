using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class TagService : ITagService
    {
        private readonly IGenericRepository<Tag> _tagRepo;
        private readonly IGenericRepository<NewsArticle> _newsRepo;

        public TagService(ITagRepository tagRepo, INewsArticleRepository newsRepo)
        {
            _tagRepo = tagRepo;
            _newsRepo = newsRepo;
        }

        public IEnumerable<Tag> GetTags(string? tagName = null)
        {
            var query = _tagRepo.GetAll();
            if (!string.IsNullOrWhiteSpace(tagName))
                query = query.Where(t => t.TagName != null && t.TagName.Contains(tagName));
            return query;
        }

        public Tag GetTagById(int id)
        {
            var tag = _tagRepo.GetById(id);
            if (tag == null) throw new Exception("Tag not found");
            return tag;
        }

        public Tag CreateTag(Tag tag)
        {
            if (string.IsNullOrWhiteSpace(tag.TagName))
                throw new Exception("TagName is required");
            if (IsDuplicateTagName(tag.TagName))
                throw new Exception("A tag with the same name already exists.");
            int maxId = _tagRepo.GetAll().Max(t => t.TagId);
            tag.TagId = maxId + 1; 
            _tagRepo.Add(tag);
            _tagRepo.Save();
            return tag;
        }

        public Tag UpdateTag(int id, Tag tag)
        {
            var existing = _tagRepo.GetById(id);
            if (existing == null) throw new Exception("Tag not found");
            if (!string.Equals(existing.TagName, tag.TagName, StringComparison.OrdinalIgnoreCase)
        && IsDuplicateTagName(tag.TagName))
            {
                throw new Exception("A tag with the same name already exists.");
            }
            existing.TagName = tag.TagName;
            existing.Note = tag.Note;
            _tagRepo.Update(existing);
            _tagRepo.Save();
            return existing;
        }
        private bool IsDuplicateTagName(string tagName, int? excludeTagId = null)
        {
            return _tagRepo
                .GetAll()
                .Any(t => t.TagName!.Trim().ToLower() == tagName.Trim().ToLower()
                       && (excludeTagId == null || t.TagId != excludeTagId));
        }

        public void DeleteTag(int id)
        {
            var existing = _tagRepo.GetById(id);
            if (existing == null) throw new Exception("Tag not found");

            // Kiểm tra nếu tag đang được dùng trong NewsArticle
            if (existing.NewsArticles.Any())
                throw new Exception("Cannot delete tag because it is used by articles");

            _tagRepo.Delete(existing);
            _tagRepo.Save();
        }

        public IEnumerable<NewsArticleByTagDto> GetArticlesByTag(int tagId)
        {
            // Lấy tag
            var tag = _tagRepo.GetById(tagId);
            if (tag == null) throw new Exception("Tag not found");

            var articles = tag.NewsArticles; // trực tiếp collection navigation

            var result = articles.Select(a => new NewsArticleByTagDto
            {
                NewsArticleId = a.NewsArticleId,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                CreatedDate = (DateTime)a.CreatedDate,
                NewsContent = a.NewsContent,
                NewsSource = a.NewsSource,
                CategoryId = (short)a.CategoryId,
                CategoryName = a.Category != null ? a.Category.CategoryName : "",
                NewsStatus = (bool)a.NewsStatus,
                CreatedById = (short)a.CreatedById,
                CreatedByName = a.CreatedBy != null ? a.CreatedBy.AccountName : "",
                UpdatedById = a.UpdatedById,
                ModifiedDate = a.ModifiedDate,
                Tags = a.Tags != null
                    ? a.Tags.Select(t => new TagDto
                    {
                        TagId = t.TagId,
                        TagName = t.TagName,
                        Note = t.Note
                    }).ToList()
                    : new List<TagDto>()
            });

            return result;
        }
    }
}
