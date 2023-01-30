using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analyzer;

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

    public static bool TryGetNamedType(this SemanticModel model, ExpressionSyntax syntax, out INamedTypeSymbol? namedTypeSymbol)
    {
        if (model.GetTypeInfo(syntax).Type is INamedTypeSymbol symbol)
        {
            namedTypeSymbol = symbol;
            return true;
        }

        namedTypeSymbol = null;
        return false;
    }
}

internal static class MethodHelpers
{
    public static ImmutableHashSet<INamedTypeSymbol> GetReturnedTypes(this IMethodSymbol methodSymbol, Compilation compilation, CancellationToken cancellation)
    {
        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var syntaxReference in methodSymbol.DeclaringSyntaxReferences)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxReference.SyntaxTree);

            var node = (MethodDeclarationSyntax)syntaxReference.GetSyntax(cancellation);

            if (node.Body is not null)
            {
                var returnStatements = node.DescendantNodes()
                    .OfType<ReturnStatementSyntax>();

                foreach (var returnStatement in returnStatements)
                {
                    if (semanticModel.TryGetNamedType(returnStatement.Expression, out var namedTypeSymbol))
                    {
                        builder.Add(namedTypeSymbol);
                    }
                }
            }
            else if (node.ExpressionBody is not null)
            {
                if (semanticModel.TryGetNamedType(node.ExpressionBody.Expression, out var namedTypeSymbol))
                {
                    builder.Add(namedTypeSymbol);
                }
            }
        }

        return builder.ToImmutable();
    }
}