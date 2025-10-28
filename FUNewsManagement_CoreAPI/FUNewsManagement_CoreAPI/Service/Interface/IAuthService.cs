using FUNewsManagement_CoreAPI.Models.Responses;
using System.Security.Claims;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface IAuthService
    {
        LoginResponse Login(string email, string password);
        string Refresh(string refreshToken, string token);
    }

}
