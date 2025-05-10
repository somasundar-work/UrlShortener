using System.Threading.RateLimiting;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Asp.Versioning;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using Soms.Shared.Cors;

var builder = WebApplication.CreateBuilder(args);

#region Configure AWS Options
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
#endregion

builder
    .Services.AddFastEndpoints()
    .AddVersioning(o =>
    {
        o.DefaultApiVersion = new(1.0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
    });
VersionSets.CreateApi(">>ShortenerApi<<", v => v.HasApiVersion(new(1.0)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.ConfigureAppCors();
builder.Services.AddResponseCompression();

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
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

app.UseFastEndpoints(c =>
{
    // c.Versioning.DefaultVersion = 1;
    // c.Versioning.Prefix = "v";
    // c.Versioning.PrependToRoute = true;
    // c.Endpoints.Configurator = ep =>
    // {
    //     ep.AllowAnonymous();
    //     ep.Options(b => b.RequireCors());
    //     ep.Options(b => b.RequireHost("https://example.com"));
    // };
    c.Endpoints.RoutePrefix = "api";
});

app.Run();
