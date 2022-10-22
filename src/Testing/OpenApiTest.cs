namespace RendleLabs.OpenApi.Testing;

public class OpenApiTest
{
    public HttpMethod Method { get; set; }
    public string Uri { get; set; }
    public OpenApiTestBody? RequestBody { get; internal init; }
    public IReadOnlyDictionary<string, string[]> Headers { get; internal init; }
    public OpenApiTestExpect Expect { get; internal init; }
}

public class OpenApiSequenceTest : OpenApiTest
{
    public string OutputName { get; set; }
    public Dictionary<string, string[]> ResponseHeaders { get; set; }
    public byte[] ResponseBody { get; set; }
    public int ResponseStatus { get; set; }
}