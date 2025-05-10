using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UrlShortener.API.Response
{
    public class Result<T>
    {
        [JsonPropertyOrder(1)]
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyOrder(2)]
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyOrder(4)]
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyOrder(3)]
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        public static Result<T> Success(T data, string message) =>
            new()
            {
                IsSuccess = true,
                Status = "Success",
                Data = data,
                Message = message,
            };

        public static Result<T> Failure(string message) =>
            new()
            {
                IsSuccess = false,
                Status = "Failure",
                Message = message,
            };

        public static Result<T> Error(string message) =>
            new()
            {
                IsSuccess = false,
                Status = "Error",
                Message = message,
            };
    }
}
