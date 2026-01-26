using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace CoffeeShopApi.Shared;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; } = string.Empty;
    public int Status { get; set; } // HTTP Status code

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    // Thành công
    public static ApiResponse<T> Ok(T? data = default, string? message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Status = (int)HttpStatusCode.OK,
            Data = data
        };
    }

    // Thất bại chung (BadRequest)
    public static ApiResponse<T> Fail(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Errors = new List<string> { error }
        };
    }

    // Thất bại với nhiều lỗi
    public static ApiResponse<T> Fail(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Errors = errors
        };
    }

    // Không tìm thấy (NotFound)
    public static ApiResponse<T> NotFound(string error = "Không tìm thấy dữ liệu")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Errors = new List<string> { error }
        };
    }

    // Lỗi Server (500)
    public static ApiResponse<T> ServerError(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Errors = new List<string> { error }
        };
    }
    
    // Unauthorized
    public static ApiResponse<T> Unauthorized(string error = "Bạn cần đăng nhập để truy cập")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Unauthorized",
            Status = (int)HttpStatusCode.Unauthorized,
            Errors = new List<string> { error }
        };
    }

    // Forbidden
    public static ApiResponse<T> Forbidden(string error = "Bạn không có quyền truy cập")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Forbidden",
            Status = (int)HttpStatusCode.Forbidden,
            Errors = new List<string> { error }
        };
    }
}