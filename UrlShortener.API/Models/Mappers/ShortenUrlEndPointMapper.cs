using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using UrlShortener.API.Models.Dtos;

namespace UrlShortener.API.Models.Mappers
{
    public class ShortenUrlEndPointMapper(IConfiguration _config, IDynamoDBContext _database)
        : Mapper<ShortenUrlDto, ShortenUrlRes, UrlsTable>
    {
        public override UrlsTable ToEntity(ShortenUrlDto r)
        {
            var shortCode = GenerateShortCode();
            shortCode.Wait();
            var expiry = r.Expiration ?? DateTime.MaxValue.AddDays(-7);
            return new()
            {
                ShortCode = shortCode.Result,
                LongUrl = r.LongUrl,
                ExpiryDate = expiry,
                CreatedAt = DateTime.UtcNow,
                DeletionDate = expiry.AddDays(7),
                IsActive = true,
            };
        }

        public override ShortenUrlRes FromEntity(UrlsTable e)
        {
            var baseUrl = _config.GetValue<string>("LinkBaseUrl");
            var shortLink = new Uri($"{baseUrl}/{e.ShortCode}");
            return new() { ShortCode = e.ShortCode, Link = shortLink };
        }

        private async Task<bool> CheckAlias(string customAlias)
        {
            var url = await _database.LoadAsync<UrlsTable>(customAlias);
            return url == null;
        }

        private async Task<string> GenerateShortCode()
        {
            var shortCode = MD5.HashData(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))
                .Select(x => x.ToString("x2"))
                .Aggregate((x, y) => x + y)[..6];
            var notExist = await CheckAlias(shortCode);
            return notExist ? shortCode : await GenerateShortCode();
        }
    }
}
