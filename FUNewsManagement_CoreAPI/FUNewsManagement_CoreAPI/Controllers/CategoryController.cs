using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Models.Responses;
using FUNewsManagement_CoreAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FUNewsManagement_CoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        // add log service later
        private readonly ILogService _logService;

        public CategoryController(ICategoryService categoryService, ILogService logService)
        {
            _categoryService = categoryService;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult GetCategories(string? name, string? description)
        {
            var categories = _categoryService.GetCategories(name, description);
            return Ok(new APIResponse<IEnumerable<CategoryWithCount>>
            {
                StatusCode = 200,
                Message = "Categories retrieved successfully",
                Data = categories
            });
        }

        [HttpPost]
        [Authorize(Roles = "1,999")]
        public IActionResult CreateCategory(Category category)
        {
            try
            {
                var created = _categoryService.CreateCategory(category);
                // Log the creation action
                if (created != null)
                {
                    var AccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    _logService.AddLog(AccountId, "Category", "Create", null, created);
                }
                return Ok(new APIResponse<Category>
                {
                    StatusCode = 201,
                    Message = "Category created successfully",
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
        [HttpGet]
        [Authorize(Roles = "1,999")]
        [Route("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            var category = _categoryService.GetCategories(null, null).FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound(new APIResponse<string>
                {
                    StatusCode = 404,
                    Message = "Category not found",
                    Data = null
                });
            }
            return Ok(new APIResponse<CategoryWithCount>
            {
                StatusCode = 200,
                Message = "Category retrieved successfully",
                Data = category
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "1,999")]
        public IActionResult UpdateCategory(short id, Category category)
        {
            try
            {
                var existingCategory = _categoryService.GetCategories(null, null).FirstOrDefault(c => c.CategoryId == id);
                var updated = _categoryService.UpdateCategory(id, category);
                // Log the update action
                if (updated != null)
                {
                    var AccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    _logService.AddLog(AccountId, "Category", "Update", existingCategory, updated);
                }
                return Ok(new APIResponse<Category>
                {
                    StatusCode = 200,
                    Message = "Category updated successfully",
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
        [Authorize(Roles = "1,999")]
        public IActionResult DeleteCategory(short id)
        {
            try
            {
                var existingCategory = _categoryService.GetCategories(null, null).FirstOrDefault(c => c.CategoryId == id);
                _categoryService.DeleteCategory(id);
                // Log the deletion action
                if (existingCategory != null)
                {
                    var AccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    _logService.AddLog(AccountId, "Category", "Delete", existingCategory, null);
                }
                return Ok(new APIResponse<string>
                {
                    StatusCode = 200,
                    Message = "Category deleted successfully",
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

        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "1,999")]
        public IActionResult ToggleCategoryVisibility(short id, [FromQuery] bool isActive)
        {
            try
            {
                var existingCategory = _categoryService.GetCategories(null, null).FirstOrDefault(c => c.CategoryId == id);
                _categoryService.ToggleCategoryVisibility(id, isActive);
                // Log the visibility toggle action
                if (existingCategory != null) {
                    var AccountId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                    var updatedCategory = _categoryService.GetCategories(null, null).FirstOrDefault(c => c.CategoryId == id);
                    _logService.AddLog(AccountId, "Category", "Toggle Visibility", existingCategory, updatedCategory);
                }
                return Ok(new APIResponse<string>
                {
                    StatusCode = 200,
                    Message = "Category visibility updated",
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
