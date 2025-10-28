using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Impl;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsArticleService _newsService;
        private readonly ILogService _logService;
        private readonly IViewRepository _viewRepo;

        public NewsController(INewsArticleService newsService, ILogService logService, IViewRepository _viewRepo)
        {
            _newsService = newsService;
            _logService = logService;
            this._viewRepo = _viewRepo;
        }

        // GET: api/News
        [HttpGet]
        [EnableQuery]
        [Authorize(Roles = "1,999")]
        public IQueryable<NewsArticleDto> Get()
        {
            return _newsService.GetArticles();
        }
        [HttpGet("Public")]
        [EnableQuery]
        public IQueryable<NewsArticleDto> GetPublic()
        {
            return _newsService.GetArticles()
                .Where(a => a.NewsStatus == true);
        }


        // GET: api/News/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var article = _newsService.GetArticles().FirstOrDefault(a => a.NewsArticleId == id);
            if (article == null)
                return NotFound(new APIResponse<string>(404, "Article not found"));
            var viewLog = new NewsView();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            viewLog.NewsArticleId = id;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                viewLog.ViewedById = short.Parse(userIdClaim);
            }
            viewLog.ViewedAt = DateTime.UtcNow;
            _viewRepo.Add(viewLog);
            _viewRepo.Save();
            return Ok(new APIResponse<NewsArticleDto>(200, "Success", article));
        }

        // POST: api/News
        [HttpPost]
        [Authorize(Roles = "1,999")]
        public IActionResult Create([FromBody] NewsArticleCreateRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new APIResponse<string>(401, "User not authenticated"));

                int currentUserId = int.Parse(userIdClaim);

                var article = new NewsArticle
                {
                    NewsTitle = request.NewsTitle,
                    Headline = request.Headline,
                    NewsContent = request.NewsContent,
                    NewsSource = request.NewsSource,
                    CategoryId = request.CategoryId,
                    NewsStatus = request.NewsStatus
                };

                var created = _newsService.CreateArticle(article, (short)currentUserId, request.TagIds);
                var createdDto = _newsService.GetArticles().FirstOrDefault(a => a.NewsArticleId == created.NewsArticleId);

                if (created != null)
                {

                    // Log the creation action
                    _logService.AddLog(currentUserId, "News", "Create", null, createdDto);
                }
                return Ok(new APIResponse<NewsArticleDto>(200, "Article created", createdDto));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }


        // PUT: api/News/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "1,999")]
        public IActionResult Update(string id, [FromBody] NewsArticleUpdateRequest request)
        {
            try
            {
                var existingArticle = _newsService.GetArticles().FirstOrDefault(a => a.NewsArticleId == id);
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new APIResponse<string>(401, "User not authenticated"));

                int currentUserId = int.Parse(userIdClaim); 

                var article = new NewsArticle
                {
                    NewsTitle = request.NewsTitle,
                    Headline = request.Headline,
                    NewsContent = request.NewsContent,
                    NewsSource = request.NewsSource,
                    CategoryId = request.CategoryId,
                    NewsStatus = request.NewsStatus
                };

                var updated = _newsService.UpdateArticle(id, article, (short)currentUserId, request.TagIds);
                var updatedDto = new NewsArticleDto
                {
                    NewsArticleId = updated.NewsArticleId,
                    NewsTitle = updated.NewsTitle,
                    Headline = updated.Headline,
                    NewsContent = updated.NewsContent,
                    NewsSource = updated.NewsSource,
                    CategoryId = (short)updated.CategoryId,
                    CategoryName = updated.Category?.CategoryName ?? "",
                    NewsStatus = (bool)updated.NewsStatus,
                    CreatedByName = updated.CreatedBy.AccountName??"",
                    CreatedDate = (DateTime)updated.CreatedDate,
                    TagIds = updated.Tags?.Select(nt => nt.TagId).ToList()
                };
                // Log the update action
                _logService.AddLog(currentUserId, "News", "Update", existingArticle, updatedDto);
                return Ok(new APIResponse<NewsArticleDto>(200, "Article updated", updatedDto));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }

        // DELETE: api/News/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "1,999")]
        public IActionResult Delete(string id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var existingArticle = _newsService.GetArticles().FirstOrDefault(a => a.NewsArticleId == id);

                _newsService.DeleteArticle(id);
                //log the delete action
                if (!string.IsNullOrEmpty(userIdClaim) && existingArticle != null)
                {
                    int currentUserId = int.Parse(userIdClaim);
                    _logService.AddLog(currentUserId, "News", "Delete", existingArticle, null);
                }
                return Ok(new APIResponse<string>(200, "Article deleted"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }

        // POST: api/News/Duplicate/{id}
        [HttpPost("Duplicate/{id}")]
        [Authorize(Roles = "1,999")]
        public IActionResult Duplicate(string id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new APIResponse<string>(401, "User not authenticated"));

                int currentUserId = int.Parse(userIdClaim);
                var duplicated = _newsService.DuplicateArticle(id, (short)currentUserId);
                //log the duplicate action
                _logService.AddLog(currentUserId, "News", "Duplicate", null, duplicated);
                return Ok(new APIResponse<NewsArticle>(200, "Article duplicated", duplicated));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }

    }

    // DTO Request cho Create
    public class NewsArticleCreateRequest
    {
        [Required]
        public string NewsTitle { get; set; }
        [Required]
        public string Headline { get; set; }
        [Required]
        public string NewsContent { get; set; }
        [Required]
        public string NewsSource { get; set; }
        [Required]
        public short CategoryId { get; set; }
        [Required]
        public bool NewsStatus { get; set; }
        public short? CurrentUserId { get; set; }
        public List<int>? TagIds { get; set; }
    }

    // DTO Request cho Update
    public class NewsArticleUpdateRequest
    {
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public short CategoryId { get; set; }
        public bool NewsStatus { get; set; }
        public short? CurrentUserId { get; set; }
        public List<int>? TagIds { get; set; }
    }
}

