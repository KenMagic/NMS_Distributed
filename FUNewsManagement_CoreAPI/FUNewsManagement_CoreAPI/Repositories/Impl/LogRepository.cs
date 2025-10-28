using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class LogRepository : GenericRepository<AuditLog>, ILogRepository
    {
        public LogRepository(FunewsManagementContext context) : base(context)
        {
        }
    }
}
