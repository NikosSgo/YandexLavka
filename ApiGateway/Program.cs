using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins is null || allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("gateway", new OpenApiInfo
    {
        Title = "YandexLavka API Gateway",
        Version = "v1",
        Description = "Reverse proxy entrypoint for YandexLavka microservices."
    });
});

builder.Services.AddHttpClient("swagger-docs", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/internal/{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "YandexLavka Gateway Docs";
    options.SwaggerEndpoint("/swagger/gateway/swagger.json", "Gateway Aggregate");
    options.SwaggerEndpoint("/swagger/users/v1/swagger.json", "User Service API v1");
    options.SwaggerEndpoint("/swagger/warehouse/v1/swagger.json", "Warehouse Service API v1");
    options.DefaultModelsExpandDepth(-1);
});

app.UseCors("Default");

app.MapGet("/", () => Results.Json(new
{
    service = "api-gateway",
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapHealthChecks("/healthz");

app.MapGet("/swagger/gateway/swagger.json", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("swagger-docs");
    var reader = new OpenApiStreamReader();

    var sources = new (string ServiceName, string Url)[]
    {
        ("User Service", "http://userservice-api:8080/swagger/v1/swagger.json"),
        ("WareHouse Service", "http://warehouse-service-api:8080/swagger/v1/swagger.json")
    };

    var aggregateDocument = new OpenApiDocument
    {
        Info = new OpenApiInfo
        {
            Title = "YandexLavka Aggregated API",
            Version = "v1",
            Description = "Combined API surface of all downstream services."
        },
        Servers = new List<OpenApiServer> { new() { Url = "/" } },
        Paths = new OpenApiPaths(),
        Components = new OpenApiComponents
        {
            Schemas = new Dictionary<string, OpenApiSchema>(),
            Responses = new Dictionary<string, OpenApiResponse>(),
            Parameters = new Dictionary<string, OpenApiParameter>(),
            Headers = new Dictionary<string, OpenApiHeader>(),
            RequestBodies = new Dictionary<string, OpenApiRequestBody>(),
            Links = new Dictionary<string, OpenApiLink>(),
            Callbacks = new Dictionary<string, OpenApiCallback>(),
            SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>()
        },
        Tags = new List<OpenApiTag>()
    };

    foreach (var source in sources)
    {
        try
        {
            await using var stream = await httpClient.GetStreamAsync(source.Url);
            var document = reader.Read(stream, out _);

            foreach (var path in document.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    operation.Value.Tags ??= new List<OpenApiTag>();
                    if (operation.Value.Tags.All(t => t.Name != source.ServiceName))
                    {
                        operation.Value.Tags.Add(new OpenApiTag { Name = source.ServiceName });
                    }
                }

                aggregateDocument.Paths[path.Key] = path.Value;
            }

            MergeComponents(aggregateDocument.Components.Schemas, document.Components.Schemas);
            MergeComponents(aggregateDocument.Components.Responses, document.Components.Responses);
            MergeComponents(aggregateDocument.Components.Parameters, document.Components.Parameters);
            MergeComponents(aggregateDocument.Components.Headers, document.Components.Headers);
            MergeComponents(aggregateDocument.Components.RequestBodies, document.Components.RequestBodies);
            MergeComponents(aggregateDocument.Components.Links, document.Components.Links);
            MergeComponents(aggregateDocument.Components.Callbacks, document.Components.Callbacks);
            MergeComponents(aggregateDocument.Components.SecuritySchemes, document.Components.SecuritySchemes);

            if (document.Tags is { Count: > 0 })
            {
                foreach (var tag in document.Tags)
                {
                    if (aggregateDocument.Tags.All(t => t.Name != tag.Name))
                    {
                        aggregateDocument.Tags.Add(tag);
                    }
                }
            }
        }
        catch
        {
            // Ignore unreachable swagger sources to keep the gateway responsive.
        }
    }

    if (aggregateDocument.Paths.Count == 0)
    {
        return Results.Problem(
            "Unable to load downstream swagger documents.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var json = aggregateDocument.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
    return Results.Text(json, "application/json");
});

app.MapReverseProxy();

app.Run();

static void MergeComponents<TKey, TValue>(
    IDictionary<TKey, TValue> target,
    IDictionary<TKey, TValue> source) where TKey : notnull
{
    if (source is null)
    {
        return;
    }

    foreach (var (key, value) in source)
    {
        target[key] = value;
    }
}
