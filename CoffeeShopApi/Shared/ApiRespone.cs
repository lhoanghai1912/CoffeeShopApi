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
    public static ApiResponse<T> Ok(T? data = default, string? message = "Thành công")
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
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status = (int)HttpStatusCode.BadRequest,
            Errors = errors
        };
    }

    // Không tìm thấy (NotFound)
    public static ApiResponse<T> NotFound(string message = "Không tìm thấy dữ liệu")
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status = (int)HttpStatusCode.NotFound,
            Data = default
        };
    }

    // Lỗi Server (500)
    public static ApiResponse<T> ServerError(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Status = (int)HttpStatusCode.InternalServerError,
            Data = default
        };
    }
}