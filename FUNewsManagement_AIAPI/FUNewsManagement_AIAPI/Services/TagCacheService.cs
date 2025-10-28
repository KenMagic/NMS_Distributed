using FUNewsManagement_AIAPI.Models;

namespace FUNewsManagement_AIAPI.Services
{
    public class TagCacheService
    {
        private readonly Dictionary<string, TagLearningCache> _cache = new();

        public void UpdateCache(IEnumerable<string> selectedTags)
        {
            foreach (var tag in selectedTags)
            {
                var key = tag.ToLowerInvariant();
                if (_cache.ContainsKey(key))
                    _cache[key].Frequency++;
                else
                    _cache[key] = new TagLearningCache { Tag = tag };
            }
        }

        public IEnumerable<string> GetPopularTags(int top = 5)
        {
            return _cache.Values
                .OrderByDescending(c => c.Frequency)
                .Take(top)
                .Select(c => c.Tag);
        }
    }
}
