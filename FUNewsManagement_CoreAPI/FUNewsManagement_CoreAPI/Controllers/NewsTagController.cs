
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,999")]
    public class NewsTagController : ControllerBase
    {
        private readonly INewsTagService _newsTagService;
        private readonly ILogService _logService;
        public NewsTagController(INewsTagService newsTagService, ILogService logService)
        {
            _newsTagService = newsTagService;
            _logService = logService;
        }

        [HttpGet("{newsArticleId}")]
        public IActionResult GetTags(string newsArticleId)
        {
            try
            {
                var tagIds = _newsTagService.GetTagsByArticle(newsArticleId);
                return Ok(new APIResponse<List<int>>(200, "Tags retrieved successfully", tagIds));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }

        [HttpPut("{newsArticleId}")]
        public IActionResult UpdateTags(string newsArticleId, [FromBody] NewsTagUpdateDto dto)
        {
            if (dto.NewsArticleId != newsArticleId)
            {
                return BadRequest(new APIResponse<string>(400, "Article ID mismatch"));
            }

            try
            {
                var existingTags = _newsTagService.GetTagsByArticle(newsArticleId);
                _newsTagService.UpdateTags(newsArticleId, dto.TagIds);
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var updatedTags = _newsTagService.GetTagsByArticle(newsArticleId);
                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim);
                    _logService.AddLog(userId, "NewsTag", "Update", existingTags, updatedTags);
                }
                return Ok(new APIResponse<string>(200, "Tags updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }
    }
    public class NewsTagUpdateDto
    {
        public string NewsArticleId { get; set; } = null!;
        public List<int> TagIds { get; set; } = new List<int>();
    }
}

