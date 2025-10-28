using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace FUNewsManagement_CoreAPI.Repositories.Impl
{
    public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        private readonly FunewsManagementContext _context;

        public NewsArticleRepository(FunewsManagementContext context) : base(context)
        {
            _context = context;
        }
        public override IQueryable<NewsArticle> GetAll()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags);
        }


        public override NewsArticle GetById(object id)
        {
            var stringId = id.ToString();
            return _context.NewsArticles
                .Include(x => x.Category)
                .Include(x => x.CreatedBy)
                .Include(x => x.Tags)
                .FirstOrDefault(a => a.NewsArticleId == stringId);
        }



    }

}
