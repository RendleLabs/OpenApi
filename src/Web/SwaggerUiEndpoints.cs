using System.IO.Pipelines;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RendleLabs.OpenApi.Web;

internal static class SwaggerUiEndpoints
{
    public static void Map(IEndpointRouteBuilder app, StaticOpenApiOptions options,
        StaticOpenApiEndpointConventionBuilder endpointConventionBuilder)
    {
        var uiPathPrefix = options.UiPathPrefix!.Trim('/');

        endpointConventionBuilder.Add(
            app.MapGet($"{uiPathPrefix}/swagger-initializer.js", async context =>
            {
                var swaggerInitializerJs = SwaggerUiInitializerTemplate
                    .Replace("{{SWAGGER.JSON}}", $"/{options.JsonPath!.Trim('/')}")
                    .Replace("{{OAUTH2_REDIRECT_URL}}", $"{context.Request.Scheme}://{context.Request.Host}/{uiPathPrefix}/oauth2-redirect.html");
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(swaggerInitializerJs);
            }));

        endpointConventionBuilder.Add(
            app.MapGet($"{uiPathPrefix}/", async context =>
            {
                context.Response.StatusCode = 301;
                context.Response.Headers.Location = $"{uiPathPrefix}/index.html";
                await context.Response.CompleteAsync();
            }));

        endpointConventionBuilder.Add(
            app.MapGet($"{uiPathPrefix}/{{*path}}", async context =>
            {
                if (!context.Request.RouteValues.TryGetValue("path", out var path))
                {
                    path = "index.html";
                }

                var resource = $"RendleLabs.OpenApi.Web.node_modules.swagger_ui_dist.{path}";
                await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                if (stream is null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.CompleteAsync();
                    return;
                }

                context.Response.StatusCode = 200;
                await stream.CopyToAsync(context.Response.BodyWriter);
            }));
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