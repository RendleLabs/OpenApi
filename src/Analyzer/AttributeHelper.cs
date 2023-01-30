using Microsoft.CodeAnalysis;

namespace Analyzer;

internal static class AttributeHelper
{
    public static bool HasHttpMethodAttribute(this IMethodSymbol methodSymbol)
    {
        foreach (var attribute in methodSymbol.GetAttributes().AsSpan())
        {
            if (IsHttpMethodAttribute(attribute.AttributeClass)) return true;
        }

        return false;
    }

    private static bool IsHttpMethodAttribute(INamedTypeSymbol? namedTypeSymbol)
    {
        return namedTypeSymbol is not null
               && HttpMethodAttributes.Contains(namedTypeSymbol.Name)
               && namedTypeSymbol.ContainingNamespace.Is("Microsoft.AspNetCore.Mvc");
    }

    private static readonly HashSet<string> HttpMethodAttributes = new()
    {
        "HttpGetAttribute", "HttpPostAttribute", "HttpPutAttribute", "HttpPatchAttribute",
        "HttpDeleteAttribute", "HttpOptionsAttribute", "HttpHeadAttribute"
    };
}