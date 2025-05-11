using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models.Dtos;
using UrlShortener.API.Models.Mappers;
using UrlShortener.API.Response;

namespace UrlShortener.API.EndPoints
{
    public class ShortenUrl(ILogger<ShortenUrl> _logger, IDynamoDBContext _database)
        : Endpoint<ShortenUrlDto, Result<ShortenUrlRes>, ShortenUrlMapper>
    {
        public override void Configure()
        {
            _logger.LogInformation(
                "Configuring ShortenUrl EndPoint. Method: 'Post', Path: '/api/shortener', Version 1.0"
            );
            Post("/shortener");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(ShortenUrlDto req, CancellationToken ct)
        {
            _logger.LogInformation("Handling request for ShortenUrl EndPoint.");
            try
            {
                _logger.LogInformation($"Processing Request: {JsonSerializer.Serialize(req)}");
                var data = Map.ToEntity(req);
                await _database.SaveAsync(data, ct);
                var response = Map.FromEntity(data);
                _logger.LogInformation("Shortening Url Completed. Returning Response");
                var result = Result<ShortenUrlRes>.Success(response, "Url Shortened successfully");
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
    }
}
