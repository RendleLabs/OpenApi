using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Bundle;

public class ReferenceLoader : IDisposable
{
    private readonly HttpClient _client = new();
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<T> LoadAsync<T>(ReferenceInfo<T> referenceInfo) where T : IOpenApiReferenceable
    {
        string text;
        if (ReferencePath.IsHttp(referenceInfo.Path))
        {
            text = await LoadHttpAsync(referenceInfo.Path);
        }
        else
        {
            text = await LoadFileAsync(referenceInfo.Path);
        }


        if (referenceInfo.Path.Contains('#'))
        {
            return FragmentFinder.Find<T>(text, referenceInfo.Path);
        }

        var reader = new OpenApiStringReader();
        var fragment = reader.ReadFragment<T>(text, OpenApiSpecVersion.OpenApi3_0, out var diagnostic);

        if (diagnostic.Errors is { Count: > 0 })
        {
            if (diagnostic.Errors.Any(e => !e.Message.Contains("is not a valid property")))
            {
                throw new BundleException($"Error parsing {typeof(T).Name}", diagnostic);
            }
        }

        return fragment;
    }

    private async Task<string> LoadFileAsync(string path)
    {
        path = RemoveFragment(path);

        if (_cache.TryGetValue(path, out var text)) return text;

        try
        {
            text = await File.ReadAllTextAsync(path);
            _cache[path] = text;
            return text;
        }
        catch (Exception ex)
        {
            throw new BundleException("Could not load file '{path}'", ex);
        }
    }

    private async Task<string> LoadHttpAsync(string uri)
    {
        uri = RemoveFragment(uri);

        if (_cache.TryGetValue(uri, out var text)) return text;

        try
        {
            text = await _client.GetStringAsync(uri);
            _cache[uri] = text;
            return text;
        }
        catch (HttpRequestException ex)
        {
            int status = (int)ex.StatusCode.GetValueOrDefault(0);
            throw new BundleException($"GET {uri} returned status {status}", ex);
        }
    }

    private static string RemoveFragment(string path)
    {
        int hash = path.IndexOf('#');
        if (hash > 0) path = path[..hash];
        return path;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}