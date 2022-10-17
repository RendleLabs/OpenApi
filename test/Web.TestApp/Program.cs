using DotLabs.OpenApi.Web;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStaticOpenApi("openapi.yaml", new StaticOpenApiOptions
{
    Version = 3,
    JsonPath = "swagger/v1/openapi.json",
    YamlPath = "swagger/v1/openapi.yaml",
    UiPathPrefix = "swagger",
});

app.MapGet("/", () => "Hello World!");

app.Run();
