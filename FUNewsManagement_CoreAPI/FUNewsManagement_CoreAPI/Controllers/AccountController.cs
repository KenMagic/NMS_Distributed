using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "999")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogService _logService;
        public AccountController(IAccountService accountService, ILogService logService)
        {
            _accountService = accountService;
            _logService = logService;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<SystemAccount> Get()
        {
            return _accountService.GetAccounts();
        }
        [HttpGet("{id}")]
        public IActionResult GetById(short id)
        {
            var account = _accountService.GetAccounts().FirstOrDefault(a => a.AccountId == id);
            if (account == null)
                return NotFound(new { Message = "Account not found" });

            return Ok(account);
        }


        [HttpPost]
        public IActionResult CreateAccount(SystemAccount account)
        {
            try
            {
                var created = _accountService.CreateAccount(account);
                // Log the creation action
                if (created != null)
                {
                    var adminAccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    _logService.AddLog(adminAccountId, "SystemAccount", "Create", null, created);
                }
                return Ok(new APIResponse<SystemAccount>
                {
                    StatusCode = 201,
                    Message = "Account created successfully",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(short id, SystemAccount account, [FromQuery] string? currentPassword)
        {
            try
            {
                var existingAccount = _accountService.GetAccounts().FirstOrDefault(a => a.AccountId == id);
                var updated = _accountService.UpdateAccount(id, account, currentPassword);
                // Log the update action
                if (updated != null)
                    {
                    var adminAccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    _logService.AddLog(adminAccountId, "SystemAccount", "Update", existingAccount, updated);
                }
                return Ok(new APIResponse<SystemAccount>
                {
                    StatusCode = 200,
                    Message = "Account updated successfully",
                    Data = updated
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(short id)
        {
            try
            {
                var account = _accountService.GetAccounts().FirstOrDefault(a => a.AccountId == id);
                _accountService.DeleteAccount(id);
                // Log the deletion action
                var adminAccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                _logService.AddLog(adminAccountId, "SystemAccount", "Delete", account, null);
                return Ok(new APIResponse<string>
                {
                    StatusCode = 200,
                    Message = "Account deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

    }
}
