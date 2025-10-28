using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class SystemAccountRepository : GenericRepository<SystemAccount>, ISystemAccountRepository
    {
        public SystemAccountRepository(FunewsManagementContext context) : base(context)
        {
        }
    }
}
