using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Web;

public static class StaticOpenApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, string sourceFile, StaticOpenApiOptions options)
    {
        var document = LoadOpenApiDocument(app, sourceFile);

        MapEndpoints(app, options, document);

        return app;
    }

    public static IEndpointRouteBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, Stream sourceStream, StaticOpenApiOptions options)
    {
        var document = LoadOpenApiDocument(app, sourceStream);

        MapEndpoints(app, options, document);

        return app;
    }

    private static void MapEndpoints(IEndpointRouteBuilder app, StaticOpenApiOptions options, OpenApiDocument document)
    {
        var serverUrls = document.Servers.Select(s => s.Url.TrimEnd('/'));
        var serverUrlSet = new HashSet<string>(serverUrls, StringComparer.CurrentCultureIgnoreCase);
        if (options.JsonPath is { Length: > 0 })
        {
            app.MapGet(options.JsonPath, async context =>
            {
                AddRequestServer(context.Request, document, serverUrlSet);
                var json = document.SerializeAsJson(options.Version == 3 ? OpenApiSpecVersion.OpenApi3_0 : OpenApiSpecVersion.OpenApi2_0);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            });
        }

        if (options.YamlPath is { Length: > 0 })
        {
            app.MapGet(options.YamlPath, async context =>
            {
                AddRequestServer(context.Request, document, serverUrlSet);
                var yaml = document.SerializeAsYaml(options.Version == 3 ? OpenApiSpecVersion.OpenApi3_0 : OpenApiSpecVersion.OpenApi2_0);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/yaml";
                await context.Response.WriteAsync(yaml);
            });
        }
    }

    private static void AddRequestServer(HttpRequest request, OpenApiDocument document, HashSet<string> serverUrlSet)
    {
        var host = $"{request.Scheme}://{request.Host}";
        if (serverUrlSet.Contains(host)) return;
        document.Servers.Insert(0, new OpenApiServer
        {
            Url = host
        });
    }

    private static OpenApiDocument LoadOpenApiDocument(IEndpointRouteBuilder app, string sourceFile)
    {
        using var sourceStream = app.ServiceProvider.GetRequiredService<IHostEnvironment>()
            .ContentRootFileProvider
            .GetFileInfo(sourceFile)
            .CreateReadStream();

        return LoadOpenApiDocument(app, sourceStream);
    }

    private static OpenApiDocument LoadOpenApiDocument(IEndpointRouteBuilder app, Stream sourceStream)
    {
        var document = new OpenApiStreamReader().Read(sourceStream, out var diagnostic);

        if (diagnostic.Errors.Count > 0)
        {
            throw new StaticOpenApiLoadException(diagnostic.Errors);
        }

        if (diagnostic.Warnings.Count > 0
            && app.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("StaticOpenApi") is { } logger)
        {
            foreach (var warning in diagnostic.Warnings)
            {
                logger.LogWarning("{Message}: {Pointer}", warning.Message, warning.Pointer);
            }
        }

        return document;
    }
}