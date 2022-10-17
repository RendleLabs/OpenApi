using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace RendleLabs.OpenApi.Web;

internal static class ElementsEndpoint
{
    private static byte[]? _htmlBytes;

    public static void Map(IEndpointRouteBuilder app, StaticOpenApiOptions options, string specPath,
        StaticOpenApiEndpointConventionBuilder endpointConventionBuilder)
    {
        endpointConventionBuilder.Add(
            app.MapGet(options.Elements.Path!, async context =>
            {
                _htmlBytes ??= await LoadHtmlBytesFromAssembly(specPath);

                if (_htmlBytes.Length == 0)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                context.Response.ContentLength = _htmlBytes.Length;
                await context.Response.BodyWriter.WriteAsync(_htmlBytes);
                await context.Response.BodyWriter.FlushAsync();
            }));
    }

    private static async Task<byte[]> LoadHtmlBytesFromAssembly(string? specPath)
    {
        var resource = $"RendleLabs.OpenApi.Web.Resources.elements.html";
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        if (stream is null)
        {
            return Array.Empty<byte>();
        }

        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync();
        html = html.Replace("{{SPEC_URL}}", specPath);
        return Encoding.UTF8.GetBytes(html);
    }
}