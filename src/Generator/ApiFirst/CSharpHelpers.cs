using System.Text.RegularExpressions;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class CSharpHelpers
{
    private static readonly Regex NonAlphaNumeric = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    public static string ClassName(string openApiName) => PascalCase(openApiName);
    public static string PropertyName(string openApiName) => PascalCase(openApiName);

    private static string PascalCase(string openApiName)
    {
        var name = NonAlphaNumeric.Replace(openApiName, string.Empty);
        if (name is not { Length: > 0 })
        {
            throw new InvalidOperationException($"Cannot get C# name from '{openApiName}'");
        }

        if (char.IsUpper(name[0])) return name;
        if (name.Length == 1) return name.ToUpperInvariant();
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}