using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models;
using UrlShortener.API.Models.Dtos;

namespace UrlShortener.API.EndPoints
{
    public class GetLongUrl(ILogger<GetLongUrl> _logger, IDynamoDBContext _database) : Endpoint<GetLongUrlDto, IResult>
    {
        public override void Configure()
        {
            _logger.LogInformation(
                "Configuring GetLongUrlEndPoint. Method: 'Get', Path: '/api/shortener/#ShortCode#', Version 1.0"
            );
            Get("/shortener/{ShortCode}");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(GetLongUrlDto req, CancellationToken ct)
        {
            _logger.LogInformation("Handling request for GetLongUrlEndPoint.");
            _logger.LogInformation($"Processing Request: {JsonSerializer.Serialize(req)}");
            var data = await _database.LoadAsync<UrlsTable>(req.ShortCode, ct);
            if (data is null)
            {
                _logger.LogInformation("GetLongUrlEndPoint Url Not Found");
                await SendAsync(Results.NotFound(), cancellation: ct);
            }
            if (data!.ExpiryDate < DateTime.Today)
            {
                _logger.LogInformation("GetLongUrlEndPoint Url Expired");
                await SendAsync(Results.NotFound(), cancellation: ct);
            }
            _logger.LogInformation($"GetLongUrlEndPoint Redirecting Url: {data.LongUrl}");
            data.AccessCount += 1;
            data.LastAccessed = DateTime.UtcNow;
            await _database.SaveAsync(data, ct);
            await SendRedirectAsync(data.LongUrl, isPermanent: true, allowRemoteRedirects: true);
            _logger.LogInformation("Response sent successfully.");
        }
    }
}
