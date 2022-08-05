namespace RendleLabs.OpenApi.Testing;

public class OpenApiTest
{
    public IReadOnlyDictionary<string, string?> Parameters { get; internal init; }
    public OpenApiTestBody? RequestBody { get; internal init; }
    public IReadOnlyDictionary<string, string[]> Headers { get; internal init; }
    public OpenApiTestExpect Expect { get; internal init; }
}