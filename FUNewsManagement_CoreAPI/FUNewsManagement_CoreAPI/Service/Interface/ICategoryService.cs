using FUNewsManagement_CoreAPI.Models;

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface ICategoryService
    {
        IEnumerable<CategoryWithCount> GetCategories(string? name, string? description);
        Category CreateCategory(Category category);
        Category UpdateCategory(short id, Category category);
        void DeleteCategory(short id);
        void ToggleCategoryVisibility(short id, bool isActive);
    }
    public class CategoryWithCount
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDesciption { get; set; }
        public short? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public int ArticleCount { get; set; }
    }


}
