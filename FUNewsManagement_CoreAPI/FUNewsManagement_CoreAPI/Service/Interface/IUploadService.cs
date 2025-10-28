namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface IUploadService
    {
        Task<List<string>> UploadArticleImagesAsync(string articleId, List<IFormFile> files, HttpRequest request);

    }
}
