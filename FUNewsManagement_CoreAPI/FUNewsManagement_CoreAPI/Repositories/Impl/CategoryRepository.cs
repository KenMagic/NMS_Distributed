using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(FunewsManagementContext context) : base(context)
        {
        }
    }
}
