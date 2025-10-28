using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using System.Security.Claims;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class LogService : ILogService
    {
        private readonly IGenericRepository<AuditLog> _auditLogRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LogService(IGenericRepository<AuditLog> auditLogRepo)
        {
            _auditLogRepo = auditLogRepo;
        }
        public void AddLog(int? accountId, string entityName, string action, object? before, object? after)
        {
            var log = new AuditLog
            {
                AccountId = (short?)accountId,
                EntityName = entityName,
                Action = action,
                BeforeData = before != null ? System.Text.Json.JsonSerializer.Serialize(before) : null,
                AfterData = after != null ? System.Text.Json.JsonSerializer.Serialize(after) : null,
                Timestamp = DateTime.UtcNow
            };
            _auditLogRepo.Add(log);
            _auditLogRepo.Save();
        }
    }
}
