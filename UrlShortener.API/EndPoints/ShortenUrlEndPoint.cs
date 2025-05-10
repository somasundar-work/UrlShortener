using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models;
using UrlShortener.API.Models.Dtos;
using UrlShortener.API.Response;

namespace UrlShortener.API.EndPoints
{
    public class ShortenUrlEndPoint(
        ILogger<ShortenUrlEndPoint> _logger,
        IDynamoDBContext _database,
        IConfiguration _config
    ) : Endpoint<ShortenUrlDto, Result<ShortenUrlRes>>
    {
        public override void Configure()
        {
            _logger.LogInformation("Configuring ShortenUrlEndPoint. Method: 'Post', Path: '/api/shorten', Version 1.0");
            Post("/shorten");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(ShortenUrlDto req, CancellationToken ct)
        {
            _logger.LogInformation("Handling request for ShortenUrlEndPoint.");
            try
            {
                _logger.LogInformation($"Processing Request: {JsonSerializer.Serialize(req)}");
                ShortenUrlRes response = new();
                var baseUrl = _config.GetValue<string>("LinkBaseUrl");
                var shortCode = await GenerateShortCode();
                response.ShortCode = shortCode;
                response.Link = new Uri($"{baseUrl}/{shortCode}");
                UrlsTable data = new()
                {
                    ShortCode = shortCode,
                    LongUrl = req.LongUrl,
                    ExpiryDate = req.Expiration ?? DateTime.MaxValue,
                    CreatedAt = DateTime.UtcNow,
                    DeletionDate = req.Expiration ?? DateTime.MaxValue,
                    IsActive = true,
                };
                await _database.SaveAsync(data, ct);
                var result = Result<ShortenUrlRes>.Success(response, "Url Shortened successfully");
                _logger.LogInformation("Shortening Url Completed. Returning Response");
                await SendAsync(result, cancellation: ct);
                _logger.LogInformation("Response sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the request.");
                var result = Result<ShortenUrlRes>.Failure("An error occurred while processing your request.");
                await SendAsync(result, cancellation: ct);
            }
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
