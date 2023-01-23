using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class PathItemHelpers
{
    public static IEnumerable<OpenApiParameter> GetPathParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Path);

    public static IEnumerable<OpenApiParameter> GetQueryParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Query);

    public static IEnumerable<OpenApiParameter> GetQueryParameters(this OpenApiOperation operation) =>
        operation.Parameters.Where(p => p.In == ParameterLocation.Query);

    public static IEnumerable<OpenApiParameter> GetHeaderParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Header);

    public static IEnumerable<OpenApiParameter> GetCookieParameters(this OpenApiPathItem pathItem) =>
        GetParametersIn(pathItem, ParameterLocation.Cookie);

    private static IEnumerable<OpenApiParameter> GetParametersIn(OpenApiPathItem pathItem, ParameterLocation location) =>
        pathItem.Parameters.Where(p => p.In == location);

    public static IEnumerable<KeyValuePair<OperationType, OpenApiOperation>> GetOperationsWithTag(this OpenApiPathItem pathItem, string tag) =>
        pathItem.Operations
            .Where(o => o.Value.Tags.Any(t => t.Name.Equals(tag)));
}