using System.IO.Pipelines;
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

namespace DotLabs.OpenApi.Web;

internal class StaticOpenApi
{
    
}

public static class StaticOpenApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, string sourceFile, StaticOpenApiOptions options)
    {
        var document = LoadOpenApiDocumentFromContentFile(app, sourceFile);

        MapEndpoints(app, options, document);

        return app;
    }

    public static IEndpointRouteBuilder UseStaticOpenApi(this IEndpointRouteBuilder app, Assembly resourceAssembly, string resourcePath, StaticOpenApiOptions options)
    {
        var sourceStream = resourceAssembly.GetManifestResourceStream(resourcePath);
        if (sourceStream is null)
        {
            app.ServiceProvider.GetService<ILogger<StaticOpenApi>>()
                ?.LogWarning("OpenAPI embedded resource {ResourcePath} not found.", resourcePath);
            return app;
        }
        var document = LoadOpenApiDocumentFromStream(app, sourceStream);

        MapEndpoints(app, options, document);

        return app;
    }

    private static void MapEndpoints(IEndpointRouteBuilder app, StaticOpenApiOptions options, OpenApiDocument document)
    {
        document.Servers.Clear();
        var serverUrlSet = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        if (options.JsonPath is { Length: > 0 })
        {
            app.MapGet(options.JsonPath, async context =>
                {
                    AddRequestServer(context.Request, document, serverUrlSet);
                    var json = document.SerializeAsJson(options.Version == 3 ? OpenApiSpecVersion.OpenApi3_0 : OpenApiSpecVersion.OpenApi2_0);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(json);
                })
                .AllowAnonymous(options.AllowAnonymous);

            if (options.UiPathPrefix is { Length: > 0 })
            {
                var uiPathPrefix = options.UiPathPrefix.Trim('/');


                app.MapGet($"{uiPathPrefix}/swagger-initializer.js", async context =>
                    {
                        var swaggerInitializerJs = SwaggerUiInitializerTemplate
                            .Replace("{{SWAGGER.JSON}}", $"/{options.JsonPath.Trim('/')}")
                            .Replace("{{OAUTH2_REDIRECT_URL}}", $"{context.Request.Scheme}://{context.Request.Host}/{uiPathPrefix}/oauth2-redirect.html");
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync(swaggerInitializerJs);
                    })
                    .AllowAnonymous(options.AllowAnonymous);

                app.MapGet($"{uiPathPrefix}/", async context =>
                    {
                        context.Response.StatusCode = 301;
                        context.Response.Headers.Location = $"{uiPathPrefix}/index.html";
                        await context.Response.CompleteAsync();
                    })
                    .AllowAnonymous(options.AllowAnonymous);

                app.MapGet($"{uiPathPrefix}/{{*path}}", async context =>
                    {
                        if (!context.Request.RouteValues.TryGetValue("path", out var path))
                        {
                            path = "index.html";
                        }

                        var resource = $"DotLabs.OpenApi.Web.node_modules.swagger_ui_dist.{path}";
                        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                        if (stream is null)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.CompleteAsync();
                            return;
                        }

                        context.Response.StatusCode = 200;
                        await stream.CopyToAsync(context.Response.BodyWriter);
                    })
                    .AllowAnonymous(options.AllowAnonymous);
            }
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
                })
                .AllowAnonymous(options.AllowAnonymous);
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

    private const string SwaggerUiInitializerTemplate = @"window.onload = function() {
  //<editor-fold desc=""Changeable Configuration Block"">

  // the following lines will be replaced by docker/configurator, when it runs in a docker-container
  window.ui = SwaggerUIBundle({
    url: ""{{SWAGGER.JSON}}"",
    oauth2RedirectUrl: ""{{OAUTH2_REDIRECT_URL}}"",
    dom_id: '#swagger-ui',
    deepLinking: true,
    presets: [
      SwaggerUIBundle.presets.apis,
      SwaggerUIStandalonePreset
    ],
    plugins: [
      SwaggerUIBundle.plugins.DownloadUrl
    ],
    layout: ""StandaloneLayout""
  });

  //</editor-fold>
};";
}