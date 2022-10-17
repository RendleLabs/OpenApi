namespace RendleLabs.OpenApi.Web;

public class StaticOpenApiOptions
{
    public string? JsonPath { get; set; }
    public string? YamlPath { get; set; }
    public string? UiPathPrefix { get; set; }
    public int Version { get; set; } = 3;
    public RedocOptions Redoc { get; } = new();
    public ElementsOptions Elements { get; } = new();
}

public class RedocOptions
{
    public bool Enabled { get; set; }
    public string? Path { get; set; }
}

public class ElementsOptions
{
    public bool Enabled { get; set; }
    public string? Path { get; set; }
}
