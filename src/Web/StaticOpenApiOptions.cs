namespace DotLabs.OpenApi.Web;

public class StaticOpenApiOptions
{
    public string? JsonPath { get; set; }
    public string? YamlPath { get; set; }
    public string? UiPathPrefix { get; set; }
    public int Version { get; set; } = 3;
    public bool AllowAnonymous { get; set; } = true;
}
