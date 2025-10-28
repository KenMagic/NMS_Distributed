namespace FUNewsManagement_CoreAPI.Models.Responses
{
    public class APIResponse<T>
    {
        public int StatusCode { get; set; }  
        public string Message { get; set; } 
        public T? Data { get; set; }         

        public APIResponse() { }

        public APIResponse(int statusCode, string message, T? data = default)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }
    }
}

