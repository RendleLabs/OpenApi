namespace RendleLabs.OpenApi.Web;

public class StaticOpenApiOptions
{
    public string? JsonPath { get; set; }
    public string? YamlPath { get; set; }
    public int Version { get; set; } = 3;
}