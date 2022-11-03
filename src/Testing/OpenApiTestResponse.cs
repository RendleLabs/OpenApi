namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestResponse
{
    public int Status { get; set; }
    public string? Body { get; set; }
    public IReadOnlyDictionary<string, string[]>? Headers { get; set; }
    public string? ContentType { get; set; }
    public string? OutputName { get; set; }

    public string? GetOutput(string path)
    {
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).AsSpan();
        if (parts.Length < 2) throw new InvalidOperationException();
        if (parts[0].Equals("Headers")) return GetOutputHeader(parts.Slice(1));
        else throw new InvalidOperationException();
    }

    private string? GetOutputHeader(Span<string> slice)
    {
        if (Headers is null) return null;
        if (Headers.TryGetValue(slice[0], out var headers) && headers?.Length > 0) return headers.FirstOrDefault();
        return null;
    }

    public override string ToString() => Status.ToString();
}
