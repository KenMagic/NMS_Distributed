using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<SystemAccount> _accountRepo;
        private readonly IConfiguration _config;

        public AuthService(ISystemAccountRepository accountRepo, IConfiguration config)
        {
            _accountRepo = accountRepo;
            _config = config;
        }

        public LoginResponse Login(string email, string password)
        {
            var user = _accountRepo.GetAll()
                .FirstOrDefault(u => u.AccountEmail == email && u.AccountPassword == password);


            // Kiểm tra admin từ appsettings
            var adminEmail = _config["AdminAccount:Email"];
            if (email == adminEmail && password == _config["AdminAccount:Password"])
            {
                user = new SystemAccount { AccountEmail = adminEmail };
                user.AccountRole = 999; // Admin
            }
            if (user == null) return null;

            string token = GenerateJwtToken(user);
            string refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Thời gian hết hạn refresh token
            if (user.AccountRole != 999)
            {
                _accountRepo.Update(user);
                _accountRepo.Save();
            }
            LoginResponse loginResponse = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
            return loginResponse;
        }

        public string Refresh(string refreshToken, string token)
        {
            //check expired token
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
                throw new SecurityTokenException("Invalid token");
            var email = principal.Identity.Name;
            var user = _accountRepo.GetAll()
                .FirstOrDefault(u => u.AccountEmail == email);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new SecurityTokenException("Invalid token");
            var newJwtToken = GenerateJwtToken(user);
            return newJwtToken;
        }

        private string GenerateJwtToken(SystemAccount user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
        new Claim(ClaimTypes.Email, user.AccountEmail),
        new Claim(ClaimTypes.Role, user.AccountRole.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Thời gian hết hạn access token
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

}
