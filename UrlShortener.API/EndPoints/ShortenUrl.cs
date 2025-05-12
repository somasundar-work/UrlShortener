using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models.Dtos;
using UrlShortener.API.Models.Mappers;
using UrlShortener.API.Response;

namespace UrlShortener.API.EndPoints
{
    public class ShortenUrl(ILogger<ShortenUrl> _log, IDynamoDBContext _context)
        : Endpoint<ShortenUrlDto, Result<ShortenUrlRes>, ShortenUrlMapper>
    {
        public override void Configure()
        {
            _log.LogInformation("Configuring ShortenUrl EndPoint. Method: 'Post', Path: '/api/shortener', Version 1.0");
            Post("/shortener");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(ShortenUrlDto req, CancellationToken ct)
        {
            _log.LogInformation("Handling request for ShortenUrl EndPoint.");
            try
            {
                _log.LogInformation("Processing Request: {SerializedData}", JsonSerializer.Serialize(req));
                var data = Map.ToEntity(req);
                await _context.SaveAsync(data, ct);
                var response = Map.FromEntity(data);
                _log.LogInformation("Shortening Url Completed. Returning Response");
                var result = Result<ShortenUrlRes>.Success(response, "Url Shortened successfully");
                await SendAsync(result, cancellation: ct);
                _log.LogInformation("Response sent successfully.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred while handling the request.");
                var result = Result<ShortenUrlRes>.Failure("An error occurred while processing your request.");
                await SendAsync(result, cancellation: ct);
            }
        }
    }
}
