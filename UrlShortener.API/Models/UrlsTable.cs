using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace UrlShortener.API.Models
{
    [DynamoDBTable("UrlShortener_UrlsTable")]
    public class UrlsTable
    {
        [DynamoDBHashKey("US_UT_PK")]
        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; } = null!;

        [DynamoDBProperty("US_UT_LU")]
        [JsonPropertyName("longUrl")]
        public string LongUrl { get; set; } = null!;

        [DynamoDBProperty("US_UT_CA")]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty("US_UT_ED")]
        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiryDate { get; set; }

        [DynamoDBProperty("US_UT_LA")]
        [JsonPropertyName("lastAccessed")]
        public DateTime? LastAccessed { get; set; }

        [DynamoDBProperty("US_UT_AC")]
        [JsonPropertyName("clicks")]
        public int AccessCount { get; set; }

        [DynamoDBProperty("US_UT_IA")]
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
