using FUNewsManagement_AnalyticsAPI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace FUNewsManagement_AnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Dashboard tổng hợp số lượng bài viết theo Category và Status.
        /// </summary>
        [HttpGet("dashboard")]
        [EnableQuery]
        public IActionResult GetDashboard()
        {
            var result = _analyticsService.GetDashboard();
            return Ok(result);
        }

        /// <summary>
        /// Danh sách bài viết Trending (nhiều lượt xem nhất).
        /// </summary>
        [HttpGet("trending")]
        [EnableQuery]
        public IActionResult GetTrending([FromQuery] int top = 5)
        {
            var result = _analyticsService.GetTrending(top);
            return Ok(result);
        }

        /// <summary>
        /// Gợi ý bài viết liên quan theo category hoặc tag.
        /// </summary>
        [HttpGet("~/api/recommend/{id}")]
        public IActionResult Recommend(string id, [FromQuery] int take = 5)
        {
            var result = _analyticsService.Recommend(id, take);
            return Ok(result);
        }

        /// <summary>
        /// Lọc bài viết theo Category, Author, Date, Status.
        /// </summary>
        [HttpGet("filter")]
        public IActionResult Filter(
            [FromQuery] short? categoryId,
            [FromQuery] short? authorId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] bool? status)
        {
            var result = _analyticsService.Filter(categoryId, authorId, from, to, status);
            return Ok(result);
        }

        /// <summary>
        /// Xuất báo cáo Excel.
        /// </summary>
        [HttpGet("export")]
        public IActionResult ExportExcel(
            [FromQuery] short? categoryId,
            [FromQuery] short? authorId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] bool? status)
        {
            var bytes = _analyticsService.ExportToExcel(categoryId, authorId, from, to, status);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "analytics_report.xlsx");
        }
    }
}
