namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestResponse
{
    public int Status { get; set; }
    public string? Body { get; set; }
    public IReadOnlyDictionary<string, string[]>? Headers { get; set; }
    public string? ContentType { get; set; }

    public override string ToString() => Status.ToString();
}
