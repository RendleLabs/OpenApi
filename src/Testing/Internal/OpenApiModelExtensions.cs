using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Testing.Internal;

public static class OpenApiModelExtensions
{
    public static bool TryGetParameter(this OpenApiPathItem pathItem, OperationType operationType, string name, [NotNullWhen(true)] out OpenApiParameter? parameter)
    {
        if (pathItem.Operations.TryGetValue(operationType, out var operation))
        {
            parameter = operation.Parameters.FirstOrDefault(p => p.Name == name);
            if (parameter is not null) return true;
        }

        parameter = pathItem.Parameters.FirstOrDefault(p => p.Name == name);
        return parameter is not null;
    }
}