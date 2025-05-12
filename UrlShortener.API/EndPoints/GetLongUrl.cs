using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models;
using UrlShortener.API.Models.Dtos;

namespace UrlShortener.API.EndPoints
{
    public class GetLongUrl(ILogger<GetLongUrl> _log, IDynamoDBContext _context) : Endpoint<GetLongUrlDto, IResult>
    {
        public override void Configure()
        {
            _log.LogInformation(
                "Configuring GetLongUrlEndPoint. Method: 'Get', Path: '/api/shortener/#ShortCode#', Version 1.0"
            );
            Get("/shortener/{ShortCode}");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(GetLongUrlDto req, CancellationToken ct)
        {
            _log.LogInformation("Handling request for GetLongUrlEndPoint.");
            _log.LogInformation("Processing Request: {SerializedData}", JsonSerializer.Serialize(req));
            var data = await _context.LoadAsync<UrlsTable>(req.ShortCode, ct);
            if (data is null)
            {
                _log.LogInformation("GetLongUrlEndPoint Url Not Found");
                await SendAsync(Results.NotFound(), cancellation: ct);
            }
            if (data!.ExpiryDate < DateTime.Today)
            {
                _log.LogInformation("GetLongUrlEndPoint Url Expired");
                await SendAsync(Results.NotFound(), cancellation: ct);
            }
            _log.LogInformation($"GetLongUrlEndPoint Redirecting Url: {data.LongUrl}");
            data.AccessCount += 1;
            data.LastAccessed = DateTime.UtcNow;
            await _context.SaveAsync(data, ct);
            await SendRedirectAsync(data.LongUrl, isPermanent: true, allowRemoteRedirects: true);
            _log.LogInformation("Response sent successfully.");
        }
    }
}
