namespace Application.DTOs.Common
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static ApiResponseDto<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, StatusCode = 200, Message = message };

        public static ApiResponseDto<T> Created(T data, string? message = null) =>
            new() { Success = true, Data = data, StatusCode = 201, Message = message };

        public static ApiResponseDto<T> Fail(string message, int statusCode = 400, IEnumerable<string>? errors = null) =>
            new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
    }
}
