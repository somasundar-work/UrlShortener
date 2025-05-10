using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UrlShortener.API.Models.Dtos
{
    public class ShortenUrlRes
    {
        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; } = null!;

        [JsonPropertyName("shortLink")]
        public Uri Link { get; set; } = null!;
    }
}
