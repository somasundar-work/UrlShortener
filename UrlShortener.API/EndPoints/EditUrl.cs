using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models;
using UrlShortener.API.Models.Dtos;
using UrlShortener.API.Response;

namespace UrlShortener.API.EndPoints
{
    public class EditUrl(ILogger<EditUrl> _log, IDynamoDBContext _context) : Endpoint<EditUrlDto, Result<bool>>
    {
        public override void Configure()
        {
            _log.LogInformation("Configuring ListUrls. Method: 'Put', Path: '/api/shortener', Version 1.0");
            Put("/shortener");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(EditUrlDto req, CancellationToken ct)
        {
            _log.LogInformation("Handling request for EditUrl EndPoint.");
            _log.LogInformation("Processing Request: {SerializedData}", JsonSerializer.Serialize(req));
            UrlsTable? data = await _context.LoadAsync<UrlsTable>(req.ShortCode, ct);
            _log.LogInformation(
                "Urls Table Data For Short Code {ShortCode}: {SerializedData}",
                req.ShortCode,
                JsonSerializer.Serialize(data)
            );
            if (data is null)
            {
                await HandleResponseAsync(false, ct);
            }
            else
            {
                data.LongUrl = req.LongUrl;
                data.ExpiryDate = req.Expiration;
                data.IsActive = req.Status;
                await _context.SaveAsync(data, ct);
                await HandleResponseAsync(true, ct);
            }
            _log.LogInformation("EditUrl Completed. Returning Response");
            _log.LogInformation("Response sent successfully.");
        }

        private async Task HandleResponseAsync(bool isSuccess, CancellationToken ct = default)
        {
            var result = Result<bool>.Success(isSuccess, "Url Shortened successfully");
            await SendAsync(result, cancellation: ct);
        }
    }
}
