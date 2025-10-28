using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class NewsAuditService : INewsAuditService
    {
        private readonly IGenericRepository<NewsArticle> _newsRepo;
        private readonly IGenericRepository<SystemAccount> _accountRepo;

        public NewsAuditService(INewsArticleRepository newsRepo,
                                ISystemAccountRepository accRepo)
        {
            _newsRepo = newsRepo;
            _accountRepo = accRepo;
        }
           public IQueryable<NewsAuditDto> GetAuditInfo()
        {
            // Lấy các news articles
            var news = _newsRepo.GetAll(); // IQueryable<NewsArticle>
            var accounts = _accountRepo.GetAll(); // IQueryable<SystemAccount>

            // Left join + select DTO
            var query = news
                .GroupJoin(accounts,
                           n => n.UpdatedById,
                           u => u.AccountId,
                           (n, us) => new { n, us })
                .SelectMany(
                    x => x.us.DefaultIfEmpty(),
                    (x, u) => new NewsAuditDto
                    {
                        NewsArticleId = x.n.NewsArticleId,
                        NewsTitle = x.n.NewsTitle,
                        UpdatedById = x.n.UpdatedById,
                        LastEditorName = u != null ? u.AccountName : null,
                        ModifiedDate = x.n.ModifiedDate
                    })
                .AsQueryable();

            return query;
        }

    }


}



