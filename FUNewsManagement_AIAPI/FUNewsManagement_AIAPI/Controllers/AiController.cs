using FUNewsManagement_AIAPI.Models;
using FUNewsManagement_AIAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_AIAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly GeminiService _gemini;
        private readonly TagCacheService _cache;

        public AiController(GeminiService gemini, TagCacheService cache)
        {
            _gemini = gemini;
            _cache = cache;
        }

        [HttpPost("suggest-tags")]
        public async Task<IActionResult> SuggestTags([FromBody] SuggestRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Content cannot be empty.");

            try
            {
                var tags = await _gemini.SuggestTagsAsync(req.Content);
                return Ok(new { tags });
            }
            catch
            {
                var fallback = _cache.GetPopularTags().Select(t => new TagSuggestionResult
                {
                    Name = t,
                    Confidence = 0.5
                }).ToList();

                return Ok(new
                {
                    tags = fallback,
                    note = "Fallback from local cache"
                });
            }
        }
    }
}
