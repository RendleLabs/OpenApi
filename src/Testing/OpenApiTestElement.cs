namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestElement
{
    public HttpMethod Method { get; set; }
    public string Uri { get; set; }
    public OpenApiTestBody? RequestBody { get; internal init; }
    public IReadOnlyDictionary<string, string[]> Headers { get; internal init; }
    public OpenApiTestExpect Expect { get; internal init; }
    public string? OutputName { get; internal init; }
}

public class OpenApiTestSequence
{
    
}