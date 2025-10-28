using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<NewsArticle> _newsRepo;

        public CategoryService(ICategoryRepository categoryRepo, INewsArticleRepository newsRepo)
        {
            _categoryRepo = categoryRepo;
            _newsRepo = newsRepo;
        }

        public IEnumerable<CategoryWithCount> GetCategories(string? name, string? description)
        {
            var query = from c in _categoryRepo.GetAll()
                        join n in _newsRepo.GetAll() on c.CategoryId equals n.CategoryId into newsGroup
                        select new CategoryWithCount
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            CategoryDesciption = c.CategoryDesciption,
                            ParentCategoryId = c.ParentCategoryId,
                            IsActive = c.IsActive ?? false,
                            ArticleCount = newsGroup.Count()
                        };

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.CategoryName.Contains(name));
            if (!string.IsNullOrEmpty(description))
                query = query.Where(c => c.CategoryDesciption.Contains(description));

            return query.ToList();
        }

        public Category CreateCategory(Category category)
        {
            if (string.IsNullOrEmpty(category.CategoryName) || string.IsNullOrEmpty(category.CategoryDesciption))
                throw new Exception("CategoryName and CategoryDescription are required.");
            if (IsDuplicateCategoryName(category.CategoryName, category.ParentCategoryId))
                throw new Exception("A category with the same name already exists under this parent.");

            _categoryRepo.Add(category);
            _categoryRepo.Save();
            return category;
        }

        public Category UpdateCategory(short Id, Category category)
        {
            var existing = _categoryRepo.GetById(Id);
            if (existing == null) throw new Exception("Category not found");

            // Kiểm tra nếu category đã có bài viết thì không cho đổi ParentCategoryId
            if (existing.ParentCategoryId != category.ParentCategoryId &&
                _newsRepo.GetAll().Any(n => n.CategoryId == Id))
            {
                throw new Exception("Cannot change ParentCategoryId because category is used by articles.");
            }
            if (!string.Equals(existing.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase) &&
    IsDuplicateCategoryName(category.CategoryName, category.ParentCategoryId))
            {
                throw new Exception("A category with the same name already exists under this parent.");
            }

            existing.CategoryName = category.CategoryName;
            existing.CategoryDesciption = category.CategoryDesciption;
            existing.ParentCategoryId = category.ParentCategoryId;
            existing.IsActive = category.IsActive;

            _categoryRepo.Update(existing);
            _categoryRepo.Save();
            return existing;
        }

        public void DeleteCategory(short Id)
        {
            if (_newsRepo.GetAll().Any(n => n.CategoryId == Id))
                throw new Exception("Cannot delete category with existing articles.");

            var category = _categoryRepo.GetById(Id);
            if (category == null) throw new Exception("Category not found");

            _categoryRepo.Delete(category);
            _categoryRepo.Save();
        }

        public void ToggleCategoryVisibility(short Id, bool isActive)
        {
            var category = _categoryRepo.GetById(Id);
            if (category == null) throw new Exception("Category not found");

            category.IsActive = isActive;
            _categoryRepo.Update(category);
            _categoryRepo.Save();
        }
        private bool IsDuplicateCategoryName(string categoryName, short? parentCategoryId, short? excludeCategoryId = null)
        {
            return _categoryRepo
                .GetAll()
                .Any(c => c.ParentCategoryId == parentCategoryId
                       && c.CategoryName.Trim().ToLower() == categoryName.Trim().ToLower()
                       && (excludeCategoryId == null || c.CategoryId != excludeCategoryId));
        }
    }

}
