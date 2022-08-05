namespace RendleLabs.OpenApi.Testing.Internal;

internal struct QueryString
{
    private List<(string, string)>? _values;

    public void Add(string key, string value)
    {
        _values ??= new List<(string, string)>();
        _values.Add((key, value));
    }

    public override string ToString()
    {
        if (_values is null) return string.Empty;
        return "?" + string.Join("&", _values.Select(Pair));
    }

    private static string Pair((string, string) pair) => $"{pair.Item1}={Uri.EscapeDataString(pair.Item2)}";
}