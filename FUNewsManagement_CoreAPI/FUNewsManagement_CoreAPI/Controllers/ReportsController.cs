using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,999")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/Reports?startDate=2025-01-01&endDate=2025-10-01
        [HttpGet]
        public IActionResult GetReports([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = _reportService.GenerateReport(startDate, endDate);
                return Ok(new APIResponse<ReportDto>(200, "Report generated successfully", report));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>(400, ex.Message));
            }
        }
    }
}
