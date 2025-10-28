using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class ViewRepository : GenericRepository<NewsView>, IViewRepository
    {
        public ViewRepository(FunewsManagementContext context) : base(context)
        {
        }
    }
}
