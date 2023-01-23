using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class ResultHelper
{
    public static ResultType[] GetStatusCodes(OpenApiDocument document, string tag)
    {
        return EnumerateStatusCodes(document, tag)
            .Distinct()
            .OrderBy(s => s.StatusCode)
            .ThenBy(s => s.Type)
            .ToArray();
    }

    private static IEnumerable<ResultType> EnumerateStatusCodes(OpenApiDocument document, string tag)
    {
        foreach (var (_, path) in document.Paths)
        {
            foreach (var (_, operation) in path.Operations)
            {
                if (!operation.Tags.Any(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))) continue;

                foreach (var (codeStr, response) in operation.Responses)
                {
                    if (int.TryParse(codeStr, out var code))
                    {
                        yield return GetResultType(code, response);
                    }
                }
            }
        }
    }

    private static ResultType GetResultType(int statusCode, OpenApiResponse response)
    {
        foreach (var (_, mediaType) in response.Content)
        {
            if (mediaType.Schema.Title is { Length: > 0 } typeName)
            {
                return new ResultType(statusCode, typeName);
            }

            if (mediaType.Schema.Type == "array")
            {
                if (mediaType.Schema.Items.Title is { Length: > 0 } arrayTypeName)
                {
                    return new ResultType(statusCode, arrayTypeName, true);
                }
            }
        }

        return new ResultType(statusCode);
    }
}

internal record ResultType(int StatusCode, string? Type = null, bool IsArray = false);
