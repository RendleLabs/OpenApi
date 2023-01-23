using System.Text.RegularExpressions;

namespace RendleLabs.OpenApi.Bundle;

internal static class ReferencePath
{
    private static readonly Regex IsHttpRegex = new Regex(@"^https?:\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsHttp(string path) => IsHttpRegex.IsMatch(path);
}