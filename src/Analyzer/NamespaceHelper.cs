using Microsoft.CodeAnalysis;

namespace Analyzer;

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