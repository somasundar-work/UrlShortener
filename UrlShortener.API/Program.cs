using System.Threading.RateLimiting;
using FastEndpoints;
using Soms.Shared.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureAppCors();
builder.Services.AddOpenApi();
builder.Services.AddResponseCompression();
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        // Custom rejection handling logic
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";

        await context.HttpContext.Response.WriteAsync(
            "Rate limit exceeded. Please try again later.",
            cancellationToken
        );
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", context.HttpContext.Connection.RemoteIpAddress);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }
        )
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseRateLimiter();

app.UseAppCors();

app.MapGet("/", () => "Url Shortener API is running!")
    .WithName("GetRoot")
    .WithTags("Root Api")
    .Produces<string>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapFastEndpoints();

app.Run();
