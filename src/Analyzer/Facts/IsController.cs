using Microsoft.CodeAnalysis;

namespace Analyzer.Facts;

public static class IsControllerExtension
{
    public static bool IsController(this INamedTypeSymbol symbol)
    {
        return InheritsControllerBase(symbol)
               || HasApiControllerAttribute(symbol);
    }

    private static bool InheritsControllerBase(INamedTypeSymbol symbol) =>
        symbol.Inherits("Microsoft.AspNetCore.Mvc.ControllerBase");

    private static bool HasApiControllerAttribute(INamedTypeSymbol symbol) =>
        symbol.GetAttributes()
            .Any(IsApiControllerAttribute);

    private static bool IsApiControllerAttribute(AttributeData a) =>
        a.AttributeClass.Name == "ApiController"
        && a.AttributeClass.ContainingNamespace.Is("Microsoft.AspNetCore.Mvc");
}