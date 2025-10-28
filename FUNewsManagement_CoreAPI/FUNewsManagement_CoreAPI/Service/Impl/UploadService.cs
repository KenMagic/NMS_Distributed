using FUNewsManagement_CoreAPI.Models;
using FUNewsManagement_CoreAPI.Repositories.Interface;
using FUNewsManagement_CoreAPI.Service.Interface;

namespace FUNewsManagement_CoreAPI.Service.Impl
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly INewsImageRepository _imageRepo;

        public UploadService(IWebHostEnvironment env, INewsImageRepository imageRepo)
        {
            _env = env;
            _imageRepo = imageRepo;
        }

        public async Task<List<string>> UploadArticleImagesAsync(string articleId, List<IFormFile> files, HttpRequest request)
        {
            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var urls = new List<string>();

            if (files == null || files.Count == 0)
                throw new ArgumentException("No files uploaded.");

            var uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "articles");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext))
                    throw new InvalidOperationException($"Invalid file type: {file.FileName}");

                if (file.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException($"File too large: {file.FileName}");

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"{request.Scheme}://{request.Host}/uploads/articles/{fileName}";
                urls.Add(url);

                _imageRepo.Add(new NewsImage
                {
                    NewsArticleId = articleId,
                    ImageUrl = url
                });
            }

            _imageRepo.Save();
            return urls;
        }
    }
}
