using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ControllerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DotLabs.OpenApi";

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var symbol = (IMethodSymbol)context.Symbol;

        if (!symbol.ContainingType.Inherits("Microsoft.AspNetCore.Mvc.ControllerBase")) return;

        if (!symbol.HasHttpMethodAttribute()) return;

    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
}

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

internal static class TypeHelper
{
    public static bool Inherits(this INamedTypeSymbol namedTypeSymbol, string fullTypeName) => Inherits(namedTypeSymbol, fullTypeName.AsSpan());

    public static bool Inherits(this INamedTypeSymbol namedTypeSymbol, ReadOnlySpan<char> fullTypeName)
    {
        var typeName = TextHelpers.GetLastToken(ref fullTypeName, '.');

        for (var baseType = namedTypeSymbol.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.Name.AsSpan() == typeName && baseType.ContainingNamespace.Is("Microsoft.AspNetCore.Mvc"))
            {
            }
        }

        return false;
    }
}

internal static class NamespaceHelper
{
    public static bool Is(this INamespaceSymbol namespaceSymbol, string name) => Is(namespaceSymbol, name.AsSpan());

    public static bool Is(this INamespaceSymbol namespaceSymbol, ReadOnlySpan<char> name)
    {
        while (name.Length > 0)
        {
            var section = TextHelpers.GetLastToken(ref name, '.');
            if (section != namespaceSymbol.Name.AsSpan())
            {
                return false;
            }

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        return namespaceSymbol.IsGlobalNamespace;
    }
}

public static class TextHelpers
{
    public static ReadOnlySpan<char> GetLastToken(ref ReadOnlySpan<char> text, char delimiter)
    {
        int index = text.LastIndexOf(delimiter);
        ReadOnlySpan<char> result;
        if (index < 0)
        {
            result = text.Slice(0);
            text = ReadOnlySpan<char>.Empty;
        }
        else
        {
            result = text.Slice(index + 1);
            text = text.Slice(0, index);
        }

        return result;
    }
}