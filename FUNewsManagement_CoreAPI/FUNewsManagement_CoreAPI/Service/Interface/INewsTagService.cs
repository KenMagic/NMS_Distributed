

namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface INewsTagService
    {
        List<int> GetTagsByArticle(string newsArticleId);
        void UpdateTags(string newsArticleId, List<int> newTagIds);
    }
}

