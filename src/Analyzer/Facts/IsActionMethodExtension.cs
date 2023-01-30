using Microsoft.CodeAnalysis;

namespace Analyzer.Facts;

public static class IsActionMethodExtension
{
    public static bool IsAction(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol.MethodKind != MethodKind.Ordinary) return false;
        if (methodSymbol.DeclaredAccessibility != Accessibility.Public) return false;
        if (!methodSymbol.ContainingType.IsController()) return false;
        if (methodSymbol.GetAttributes().Any(IsNonActionAttribute)) return false;

        return true;
    }

    private static bool IsNonActionAttribute(AttributeData a) =>
        a.AttributeClass.Name == "NonActionAttribute"
        && a.AttributeClass.ContainingNamespace.Is("Microsoft.AspNetCore.Mvc");
}