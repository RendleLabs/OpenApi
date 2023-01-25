using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public static class ModelFinder
{
    public static IEnumerable<OpenApiSchema> FindModels(OpenApiDocument document)
    {
        return FindRequestModels(document).Concat(FindResponseModels(document));
    }

    private static IEnumerable<OpenApiSchema> FindRequestModels(OpenApiDocument document)
    {
        return document.Paths.Values
            .Where(p => p.Operations.Values is { Count: > 0 })
            .SelectMany(p => p.Operations.Values)
            .Where(o => o.RequestBody?.Content is { Count: > 0 })
            .SelectMany(o => o.RequestBody.Content.Values)
            .Where(m => m.Schema is not null)
            .Select(m => FixUp(m.Schema))
            .WhereNotNull();
    }

    private static IEnumerable<OpenApiSchema> FindResponseModels(OpenApiDocument document)
    {
        return document.Paths.Values
            .Where(p => p.Operations.Values is { Count: > 0 })
            .SelectMany(p => p.Operations.Values)
            .Where(o => o.Responses is { Count: > 0 })
            .SelectMany(o => o.Responses.Values)
            .Where(r => r.Content is { Count: > 0 })
            .SelectMany(r => r.Content.Values)
            .Where(m => m.Schema is not null)
            .Select(m => FixUp(m.Schema))
            .WhereNotNull();
    }

    private static OpenApiSchema? FixUp(OpenApiSchema schema)
    {
        while (schema?.Type == "array")
        {
            schema = schema.Items;
        }

        if (schema is null) return null;

        if (schema.Title is not { Length: > 0 })
        {
            schema.Title = schema.Reference?.Id;
        }

        return schema;
    }
}

internal static class NotNullExtension
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var item in source)
        {
            if (item is not null) yield return item;
        }
    }
}