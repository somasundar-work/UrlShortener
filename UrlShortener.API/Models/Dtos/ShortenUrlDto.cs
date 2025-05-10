using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UrlShortener.API.Models.Dtos
{
    public class ShortenUrlDto
    {
        [Required]
        [JsonPropertyName("longUrl")]
        public string LongUrl { get; set; } = null!;

        [JsonPropertyName("expires")]
        public DateTime? Expiration { get; set; }
    }
}
