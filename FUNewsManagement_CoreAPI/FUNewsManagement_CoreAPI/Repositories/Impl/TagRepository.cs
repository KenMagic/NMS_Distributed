using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        private readonly FunewsManagementContext _context;

        public TagRepository(FunewsManagementContext context) : base(context)
        {
            _context = context;
        }

        public override Tag GetById(object id)
        {
            return _context.Tags
                .Include(t => t.NewsArticles)
                    .ThenInclude(a => a.Category)
                .Include(t => t.NewsArticles)
                    .ThenInclude(a => a.CreatedBy)
                .Include(t => t.NewsArticles)
                    .ThenInclude(a => a.Tags)
                .FirstOrDefault(t => t.TagId == (int)id);
        }


    }

}
