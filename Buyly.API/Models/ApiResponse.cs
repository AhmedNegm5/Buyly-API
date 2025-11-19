using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buyly.API.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }

        public string[]? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }

        public static ApiResponse<T> SucessResponse(T data, string message = "Request successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, string[]? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Successful(string message = "Request successful")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
            };
        }

        public static ApiResponse Error(string message, string[]? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponse Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
            };
        }

        public static ApiResponse Forbidden(string message = "Forbidden access")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
            };
        }
    }
}