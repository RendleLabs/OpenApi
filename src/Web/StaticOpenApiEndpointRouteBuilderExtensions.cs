using System.Reflection;
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

internal class StaticOpenApi
{
}

public static class StaticOpenApiEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, string sourceFile, StaticOpenApiOptions options)
    {
        var document = LoadOpenApiDocumentFromContentFile(app, sourceFile);

        var endpointConventionBuilder = new StaticOpenApiEndpointConventionBuilder();
        MapEndpoints(app, options, document, endpointConventionBuilder);

        return endpointConventionBuilder;
    }

    public static IEndpointConventionBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, Assembly resourceAssembly, string resourcePath,
        StaticOpenApiOptions options)
    {
        var endpointConventionBuilder = new StaticOpenApiEndpointConventionBuilder();
        var sourceStream = resourceAssembly.GetManifestResourceStream(resourcePath);
        if (sourceStream is null)
        {
            app.ServiceProvider.GetService<ILogger<StaticOpenApi>>()
                ?.LogWarning("OpenAPI embedded resource {ResourcePath} not found.", resourcePath);
            return endpointConventionBuilder;
        }

        var document = LoadOpenApiDocumentFromStream(app, sourceStream);

        MapEndpoints(app, options, document, endpointConventionBuilder);

        return endpointConventionBuilder;
    }

    private static void MapEndpoints(IEndpointRouteBuilder app, StaticOpenApiOptions options, OpenApiDocument document,
        StaticOpenApiEndpointConventionBuilder endpointConventionBuilder)
    {
        document.Servers.Clear();
        var serverUrlSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        string? specPath = null;
        if (options.JsonPath is { Length: > 0 })
        {
            specPath = options.JsonPath;
            endpointConventionBuilder.Add(
                app.MapGet(options.JsonPath, async context =>
                {
                    AddRequestServer(context.Request, document, serverUrlSet);
                    var json = document.SerializeAsJson(options.Version == 3 ? OpenApiSpecVersion.OpenApi3_0 : OpenApiSpecVersion.OpenApi2_0);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(json);
                })
            );

            if (options.UiPathPrefix is { Length: > 0 })
            {
                SwaggerUiEndpoints.Map(app, options, endpointConventionBuilder);
            }
        }

        if (options.YamlPath is { Length: > 0 })
        {
            specPath ??= options.YamlPath;
            endpointConventionBuilder.Add(
                app.MapGet(options.YamlPath, async context =>
                {
                    AddRequestServer(context.Request, document, serverUrlSet);
                    var yaml = document.SerializeAsYaml(options.Version == 3 ? OpenApiSpecVersion.OpenApi3_0 : OpenApiSpecVersion.OpenApi2_0);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/yaml";
                    await context.Response.WriteAsync(yaml);
                }));
        }

        if (specPath is { Length: > 0 })
        {
            specPath = "/" + specPath.Trim('/');
            if (options.Elements.Path is { Length: > 0 })
            {
                ElementsEndpoint.Map(app, options, specPath, endpointConventionBuilder);
            }
            else if (options.Redoc.Path is { Length: > 0 })
            {
                RedocEndpoint.Map(app, options, specPath, endpointConventionBuilder);
            }
        }
    }

    private static void AddRequestServer(HttpRequest request, OpenApiDocument document, HashSet<string> serverUrlSet)
    {
        var host = $"{request.Scheme}://{request.Host}";
        if (serverUrlSet.Add(host))
        {
            document.Servers.Insert(0, new OpenApiServer
            {
                Url = host
            });
        }
    }

    private static OpenApiDocument LoadOpenApiDocumentFromContentFile(IEndpointRouteBuilder app, string sourceFile)
    {
        using var sourceStream = app.ServiceProvider.GetRequiredService<IHostEnvironment>()
            .ContentRootFileProvider
            .GetFileInfo(sourceFile)
            .CreateReadStream();

        return LoadOpenApiDocumentFromStream(app, sourceStream);
    }

    private static OpenApiDocument LoadOpenApiDocumentFromStream(IEndpointRouteBuilder app, Stream sourceStream)
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

internal class StaticOpenApiEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly List<IEndpointConventionBuilder> _builders = new();

    public void Add(IEndpointConventionBuilder builder) => _builders.Add(builder);

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var builder in _builders)
        {
            builder.Add(convention);
        }
    }
}