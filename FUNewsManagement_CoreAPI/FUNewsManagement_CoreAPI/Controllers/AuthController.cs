using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var token = _authService.Login(request.Email, request.Password);

            if (token == null)
                return Unauthorized(new APIResponse<string>
                {
                    StatusCode = 401,
                    Message = "Invalid email or password",
                    Data = null
                });

            return Ok(new APIResponse<LoginResponse>
            {
                StatusCode = 200,
                Message = "Login successful",
                Data = token
            });
        }
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                var newToken = _authService.Refresh(request.RefreshToken, request.Token);
                return Ok(new APIResponse<string>
                {
                    StatusCode = 200,
                    Message = "Token refreshed successfully",
                    Data = newToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new APIResponse<string>
                {
                    StatusCode = 401,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class RefreshRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

}
