using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.API.Models.Dtos
{
    public class GetLongUrlDto
    {
        [Required]
        [FromRoute]
        public string ShortCode { get; set; } = null!;
    }
}
