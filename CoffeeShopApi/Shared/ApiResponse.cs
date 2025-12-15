using System.Net;
using System.Text.Json.Serialization;

namespace CoffeeShopApi.Shared;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; } = string.Empty;
    public int Status { get; set; } // HTTP Status Status

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; } // Dùng T để Swagger nhận diện được Model

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    // --- CÁC HÀM TẠO NHANH ---

    // 1. Thành công
    public static ApiResponse<T> Ok(T?  data = default, string? message = "Thành công")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Status= (int)HttpStatusCode.OK,
            Data = data
        };
    }

    // 2. Thất bại chung (BadRequest)
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status= (int)HttpStatusCode.BadRequest,
            Errors = errors
        };
    }

    // 3. Không tìm thấy (NotFound)
    public static ApiResponse<T> NotFound(string message = "Không tìm thấy dữ liệu")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status= (int)HttpStatusCode.NotFound,
            Data = default 
        };
    }
    
    // 4. Lỗi Server (500)
    public static ApiResponse<T> ServerError(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status= (int)HttpStatusCode.InternalServerError,
            Data = default
        };
    }
}