using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

    namespace FUNewsManagement_CoreAPI.Service.Impl
    {
        public class NewsArticleService : INewsArticleService
        {
            private readonly IGenericRepository<NewsArticle> _newsRepo;
            private readonly IGenericRepository<Tag> _tagRepo;
            private readonly INewsTagService _newsTagService;
            private readonly FunewsManagementContext _context;

        public NewsArticleService(INewsArticleRepository newsRepo,
                                      ITagRepository tagRepo,
                                      INewsTagService newsTagService,
            FunewsManagementContext context)
            {
                _newsRepo = newsRepo;
                _tagRepo = tagRepo;
                _newsTagService = newsTagService;
            _context = context;
        }

            // Lấy danh sách article với tagIds
            public IQueryable<NewsArticleDto> GetArticles()
            {
                var query = _newsRepo.GetAll().AsQueryable().Select(n => new NewsArticleDto
                {
                    NewsArticleId = n.NewsArticleId,
                    NewsTitle = n.NewsTitle,
                    Headline = n.Headline,
                    CreatedDate = n.CreatedDate ?? DateTime.Now,
                    NewsContent = n.NewsContent,
                    NewsSource = n.NewsSource,
                    CategoryId = n.CategoryId ?? 0,
                    CategoryName = n.Category != null ? n.Category.CategoryName : "",
                    NewsStatus = n.NewsStatus ?? false,
                    CreatedById = n.CreatedById ?? 0,
                    CreatedByName = n.CreatedBy != null ? n.CreatedBy.AccountName : "",
                    UpdatedById = n.UpdatedById,
                    ModifiedDate = n.ModifiedDate,
                    TagIds = n.Tags.Select(t => t.TagId).ToList()
                });

                return query;
            }

            // Tạo bài viết mới
            public NewsArticle CreateArticle(NewsArticle article, short currentUserId, List<int>? tagIds)
            {
            article.NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20);
            article.CreatedById = currentUserId;
                article.CreatedDate = DateTime.Now;

                // Gán tag có sẵn
                if (tagIds != null)
                {
                    article.Tags = _tagRepo.GetAll()
                        .Where(t => tagIds.Contains(t.TagId))
                        .ToList();
                }

                _newsRepo.Add(article);
                _newsRepo.Save();
                return article;
            }

        // Cập nhật bài viết
        public NewsArticle UpdateArticle(string id, NewsArticle article, short currentUserId, List<int>? tagIds)
        {
            // 1. Lấy bài viết hiện có
            var existing = _newsRepo.GetById(id);
            if (existing == null) throw new Exception("Article not found");

            // 2. Cập nhật các trường cơ bản
            existing.NewsTitle = article.NewsTitle;
            existing.Headline = article.Headline;
            existing.NewsContent = article.NewsContent;
            existing.NewsSource = article.NewsSource;
            existing.CategoryId = article.CategoryId;
            existing.NewsStatus = article.NewsStatus;
            existing.UpdatedById = currentUserId;
            existing.ModifiedDate = DateTime.Now;

            // 3. Cập nhật Tag (nếu có truyền tagIds)
            if (tagIds != null)
            {
                _newsTagService.UpdateTags(id, tagIds);
            }

            // 4. Lưu thay đổi
            _newsRepo.Update(existing);
            _newsRepo.Save();

            return existing;
        }

        // Xóa bài viết
        public void DeleteArticle(string id)
        {
            var existing = _newsRepo.GetById(id);
            if (existing == null) throw new Exception("Article not found");

            // Xóa liên kết tags trước (tránh lỗi FK)
            existing.Tags.Clear();
            _newsRepo.Update(existing);
            _newsRepo.Save();
            var views = _context.NewsViews.Where(v => v.NewsArticleId == id).ToList();
            _context.NewsViews.RemoveRange(views);
            // Sau đó mới xóa article
            _newsRepo.Delete(existing);
            _newsRepo.Save();
        }



        // Duplicate bài viết
        public NewsArticle DuplicateArticle(string id, short currentUserId)
            {
                var existing = _newsRepo.GetById(id);
                if (existing == null) throw new Exception("Article not found");

                var newArticle = new NewsArticle
                {
                    NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20),
                    NewsTitle = existing.NewsTitle,
                    Headline = existing.Headline,
                    NewsContent = existing.NewsContent,
                    NewsSource = existing.NewsSource,
                    CategoryId = existing.CategoryId,
                    NewsStatus = existing.NewsStatus,
                    CreatedById = currentUserId,
                    CreatedDate = DateTime.Now,
                    Tags = existing.Tags.ToList() // Duplicate tag
                };

                _newsRepo.Add(newArticle);
                _newsRepo.Save();
                return newArticle;
            }

        }
    public class NewsArticleDto
    {
        [Key]
        public string NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public short CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool NewsStatus { get; set; }
        public short CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public short? UpdatedById { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }
    }



