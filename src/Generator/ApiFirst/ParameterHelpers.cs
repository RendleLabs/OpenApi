using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class ParameterHelpers
{
    private static readonly Regex NonAlphaNumeric = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static string CSharpName(this OpenApiParameter parameter)
    {
        var name = NonAlphaNumeric.Replace(parameter.Name, string.Empty);
        if (name is not { Length: > 0 })
        {
            throw new InvalidOperationException($"Cannot get C# name from '{parameter.Name}'");
        }

        if (name.Length == 1) return name.ToLowerInvariant();
        if (char.IsLower(name[0])) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}