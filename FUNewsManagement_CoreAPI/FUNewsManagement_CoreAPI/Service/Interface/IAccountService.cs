using FUNewsManagement_CoreAPI.Models;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface IAccountService
    {
        IQueryable<SystemAccount> GetAccounts();
        SystemAccount CreateAccount(SystemAccount account);
        SystemAccount UpdateAccount(short id, SystemAccount update, string? currentPassword = null);
        void DeleteAccount(short id);
    }

}
