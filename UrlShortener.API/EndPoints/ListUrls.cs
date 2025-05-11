using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using UrlShortener.API.Models;
using UrlShortener.API.Models.Dtos;
using UrlShortener.API.Response;

namespace UrlShortener.API.EndPoints
{
    public class ListUrls(ILogger<ListUrls> _log, IDynamoDBContext _database)
        : Endpoint<PagedQueryRequest, Result<PaginatedResponse<UrlsTable>>>
    {
        public override void Configure()
        {
            _log.LogInformation("Configuring ListUrls. Method: 'Get', Path: '/api/shortener', Version 1.0");
            Get("/shortener");
            AllowAnonymous();
            // Version(1, 0);
            Options(x => x.WithVersionSet(">>ShortenerApi<<").MapToApiVersion(1.0));
        }

        public override async Task HandleAsync(PagedQueryRequest req, CancellationToken ct)
        {
            _log.LogInformation("Handling request for GetLongUrlEndPoint.");
            _log.LogInformation($"Processing Request: {JsonSerializer.Serialize(req)}");
            List<UrlsTable> data = [];
            List<ScanCondition> conditions = [];
            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                ScanCondition scan = new("US_UT_PK", ScanOperator.Contains, req.SearchTerm);
                conditions.Add(scan);
            }
            var search = _database.ScanAsync<UrlsTable>(conditions);
            if (search is not null)
            {
                var response = await search.GetNextSetAsync(ct);
                data = response.OrderByDescending(x => x.CreatedAt).ToList();
                var pagedRes = PaginatedResponse<UrlsTable>.CreatePage(data, req.PageNumber, req.PageSize);
                var result = Result<PaginatedResponse<UrlsTable>>.Success(pagedRes, "Record Fetched successfully");
                await SendAsync(result, cancellation: ct);
            }
            else
            {
                var result = Result<PaginatedResponse<UrlsTable>>.Failure("unable to fetch records");
                await SendAsync(result, cancellation: ct);
            }
            _log.LogInformation("Response sent successfully.");
        }
    }
}
