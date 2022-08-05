using System.Diagnostics.CodeAnalysis;
using YamlDotNet.RepresentationModel;

namespace RendleLabs.OpenApi.Testing.Internal;

internal static class YamlExtensions
{
    private static readonly Dictionary<string, YamlScalarNode> Keys = new();
    
    public static bool TryGetNode(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlNode? value)
    {
        var keyNode = KeyNode(key);
        return map.Children.TryGetValue(keyNode, out value);
    }

    public static bool TryGetScalar(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlScalarNode? value)
    {
        var keyNode = KeyNode(key);
        if (map.Children.TryGetValue(keyNode, out var node) && node is YamlScalarNode scalarNode)
        {
            value = scalarNode;
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryGetString(this YamlMappingNode map, string key, [NotNullWhen(true)] out string? value)
    {
        if (map.TryGetScalar(key, out var scalarNode))
        {
            value = scalarNode.Value;
            return value is not null;
        }

        value = default;
        return false;
    }

    public static bool TryGetInt(this YamlMappingNode map, string key, [NotNullWhen(true)] out int value)
    {
        if (map.TryGetScalar(key, out var scalarNode))
        {
            return int.TryParse(scalarNode.Value, out value);
        }

        value = default;
        return false;
    }

    public static bool TryGetMap(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlMappingNode? value)
    {
        var keyNode = KeyNode(key);
        if (map.Children.TryGetValue(keyNode, out var node) && node is YamlMappingNode mappingNode)
        {
            value = mappingNode;
            return true;
        }

        value = default;
        return false;
    }

    private static YamlScalarNode KeyNode(string key)
    {
        if (!Keys.TryGetValue(key, out var keyNode))
        {
            Keys[key] = keyNode = new YamlScalarNode(key);
        }

        return keyNode;
    }
}