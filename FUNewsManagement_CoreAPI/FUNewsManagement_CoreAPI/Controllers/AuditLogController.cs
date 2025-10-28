using FUNewsManagement_CoreAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly FunewsManagementContext _context;

        public AuditLogController(FunewsManagementContext context)
        {
            _context = context;
        }

        // GET: api/auditlog
        [HttpGet]
        [EnableQuery]
        public IQueryable<AuditLogDto> Get()
        {
            return _context.AuditLogs
                .Include(a => a.Account)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserName = a.Account != null ? a.Account.AccountName : "Unknown",
                    Action = a.Action,
                    EntityName = a.EntityName,
                    Timestamp = a.Timestamp,
                    BeforeData = a.BeforeData,
                    AfterData = a.AfterData
                });
        }
    }

    public class AuditLogDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string EntityName { get; set; } = "";
        public DateTime? Timestamp { get; set; }
        public string? BeforeData { get; set; }
        public string? AfterData { get; set; }
    }
}
