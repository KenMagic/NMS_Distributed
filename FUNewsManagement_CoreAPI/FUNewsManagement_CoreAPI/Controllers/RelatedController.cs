using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelatedController : ControllerBase
    {
        private readonly INewsRecommendationService _newsRecommendationService;

        public RelatedController(INewsRecommendationService newsRecommendationService)
        {
            _newsRecommendationService = newsRecommendationService;
        }

        [HttpGet("{newsId}")]
        public IActionResult GetRelatedNews(string newsId)
        {
            var related = _newsRecommendationService.GetRelatedNews(newsId);
            return Ok(new APIResponse<IQueryable<RelatedNewsDto>>(200, "Success", related));
        }

    }
}
