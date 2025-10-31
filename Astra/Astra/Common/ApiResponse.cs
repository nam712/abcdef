namespace YourShopManagement.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public T? Data { get; set; }

        public ApiResponse(bool success, string? message = null, List<string>? errors = null, T? data = default)
        {
            Success = success;
            Message = message ?? string.Empty;
            Errors = errors ?? new List<string>();
            Data = data;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Thành công")
        {
            return new ApiResponse<T>(true, message, new List<string>(), data);
        }

        public static ApiResponse<T> FailResponse(string message, string? error = null)
        {
            var errors = error != null ? new List<string> { error } : new List<string>();
            return new ApiResponse<T>(false, message, errors);
        }

        public static ApiResponse<T> FailResponse(string message, List<string> errors)
        {
            return new ApiResponse<T>(false, message, errors);
        }

        public static ApiResponse<T> FailResponse(string message, T? data = default)
        {
            return new ApiResponse<T>(false, message, new List<string>(), data);
        }
    }
}