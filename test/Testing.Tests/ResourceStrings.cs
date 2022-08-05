using System.Reflection;

namespace RendleLabs.OpenApi.Testing.Tests;

internal static class ResourceStrings
{
    public static string Get(string name)
    {
        name = $"{typeof(MemberDataTests).Namespace}.{name}";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        if (stream is null) throw new ArgumentException();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

internal static class ResourceStreams
{
    public static Stream Get(string name)
    {
        name = $"{typeof(MemberDataTests).Namespace}.{name}";
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        if (stream is null) throw new ArgumentException();
        return stream;
    }
}
