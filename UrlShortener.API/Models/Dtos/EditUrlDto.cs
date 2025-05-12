using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UrlShortener.API.Models.Dtos
{
    public class EditUrlDto
    {
        [Required]
        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; } = null!;

        [Required]
        [JsonPropertyName("longUrl")]
        public string LongUrl { get; set; } = null!;

        [JsonPropertyName("expires")]
        public DateTime? Expiration { get; set; }

        [JsonPropertyName("status")]
        public bool Status { get; set; }
    }
}
