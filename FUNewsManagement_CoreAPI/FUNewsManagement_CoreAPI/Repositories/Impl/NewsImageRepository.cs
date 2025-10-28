using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class NewsImageRepository : GenericRepository<NewsImage>, INewsImageRepository
    {
        public NewsImageRepository(FunewsManagementContext context) : base(context)
        {
        }
    }

}
