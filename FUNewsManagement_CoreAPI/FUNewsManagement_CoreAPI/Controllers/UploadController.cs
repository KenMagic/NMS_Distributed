using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("{articleId}")]
        [Authorize(Roles = "1,999")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadImages(string articleId, [FromForm] List<IFormFile> files)
        {
            try
            {
                var urls = await _uploadService.UploadArticleImagesAsync(articleId, files, Request);
                return Ok(new
                {
                    Message = "Images uploaded successfully",
                    ImageUrls = urls
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
