using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FUNewsManagement_AIAPI.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? throw new Exception("Gemini API key missing!");
            _http = new HttpClient();
        }

        /// <summary>
        /// Gợi ý tag mới dựa trên nội dung bài viết và danh sách tag hiện tại
        /// </summary>
        /// <param name="content">Nội dung bài viết</param>
        /// <param name="existingTags">Danh sách tag hiện tại (TagName)</param>
        /// <returns>Danh sách TagDto (TagId = 0 với tag mới)</returns>
        public async Task<List<TagDto>> SuggestTagsAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<TagDto>();
            var curl = "http://localhost:5093/api/Tag"; // URL thẳng tới CoreAPI
            var resp = await _http.GetAsync(curl);
            var cjson = await resp.Content.ReadAsStringAsync();
            var apiResp = JsonSerializer.Deserialize<APIResponse<List<TagDto>>>(cjson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            List<TagDto>existingTags = apiResp?.Data ?? new List<TagDto>();
            string existingTagStr = existingTags != null && existingTags.Count > 0
                ? string.Join(", ", existingTags.Select(t => t.TagName))
                : "";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var prompt = $@"
Return a valid JSON array of up to 5 concise topic tags for the following article content.
Each tag should be 1–2 words, in English, no explanation or markdown.
MUST choose many tags of the existing tags: [{existingTagStr}].
Return only valid JSON array, e.g. [""AI"", ""Education"", ""Technology""].

Content:
""{content}""
";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, request);

            if (!response.IsSuccessStatusCode)
                return new List<TagDto> { new TagDto { TagId = 0, TagName = $"Error: {response.StatusCode}" } };

            var responseText = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                // Clean up text
                text = text.Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                           .Replace("```", "")
                           .Replace("\r", "")
                           .Replace("\n", "")
                           .Trim();

                // Parse JSON array
                List<string> tags;
                try
                {
                    tags = JsonSerializer.Deserialize<List<string>>(text) ?? new List<string>();
                }
                catch
                {
                    // fallback: split by commas or newlines
                    tags = text.Split(',', '\n', '-', '•')
                               .Select(t => t.Trim(' ', '*', '#', '.', ':'))
                               .Where(t => !string.IsNullOrWhiteSpace(t))
                               .Take(5)
                               .ToList();
                }

                var existingTagNamesLower = existingTags?.Select(t => t.TagName.ToLower()).ToHashSet() ?? new HashSet<string>();

                // Chỉ giữ những tag có trong existingTags
                var matchedTags = tags.Where(t => existingTagNamesLower.Contains(t.ToLower())).ToList();

                // Trả về TagDto với TagId = 0 (hoặc giữ tagId thật từ existingTags nếu muốn)
                return matchedTags.Select(t => new TagDto { TagId = 0, TagName = t }).ToList();
            }
            catch
            {
                return new List<TagDto> { new TagDto { TagId = 0, TagName = "Parsing error" } };
            }
        }
        public class TagDto
        {
            public int TagId { get; set; }    // nếu tag mới, để = 0
            public string TagName { get; set; }
        }
        public class APIResponse<T>
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }
    }
}
