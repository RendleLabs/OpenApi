namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestRequest
{
    public HttpMethod Method { get; init; } = null!;
    public string? Server { get; init; }
    public string Path { get; init; } = null!;
    public string? Body { get; init; }
    public string? ContentType { get; init; }
    public IReadOnlyDictionary<string,string[]>? Headers { get; init; }

    public override string ToString() => $"{Method} {ToUri()}";

    public Uri ToUri() =>
        Server is { Length: > 0 }
            ? new Uri($"{Server.TrimEnd('/')}/{Path.TrimStart('/')}", UriKind.Absolute)
            : new Uri(Path, UriKind.Relative);
}