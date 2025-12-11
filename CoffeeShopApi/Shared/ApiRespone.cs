using System.Net;
using System.Text.Json.Serialization;

namespace CoffeeShopApi.Shared;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Code { get; set; } // HTTP Status Code (200, 400, 401...)

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Data { get; set; }

    // Hàm tạo nhanh phản hồi Thành công
    public static ApiResponse Ok(object data, string message = "Thành công")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Code = 200,
            Data = data
        };
    }

    // Hàm tạo nhanh phản hồi Thất bại
    public static ApiResponse NotFoundException(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Code = (int)HttpStatusCode.NotFound
        };
    }


    public static ApiResponse BadRequestException(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Code = (int)HttpStatusCode.BadRequest
        };
    }
}