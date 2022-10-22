using System.Diagnostics.CodeAnalysis;
using YamlDotNet.RepresentationModel;

namespace RendleLabs.OpenApi.Testing.Internal;

internal static class YamlExtensions
{
    private static readonly Dictionary<string, YamlScalarNode> Keys = new();
    
    public static bool TryGetNode(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlNode? node)
    {
        var keyNode = KeyNode(key);
        return map.Children.TryGetValue(keyNode, out node);
    }

    public static bool TryGetScalar(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlScalarNode? scalarNode)
    {
        var keyNode = KeyNode(key);
        if (map.Children.TryGetValue(keyNode, out var node) && node is YamlScalarNode x)
        {
            scalarNode = x;
            return true;
        }

        scalarNode = default;
        return false;
    }

    public static bool TryGetSequence(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlSequenceNode? sequenceNode)
    {
        var keyNode = KeyNode(key);
        if (map.Children.TryGetValue(keyNode, out var node) && node is YamlSequenceNode x)
        {
            sequenceNode = x;
            return true;
        }

        sequenceNode = default;
        return false;
    }

    public static IEnumerable<KeyValuePair<TKey, TValue>> ChildrenOfType<TKey, TValue>(this YamlMappingNode node)
    {
        foreach (var (key, value) in node.Children) 
        {
            if (key is TKey tkey && value is TValue tvalue)
            {
                yield return new KeyValuePair<TKey, TValue>(tkey, tvalue);
            }
        }
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

    public static bool TryGetMap(this YamlMappingNode map, string key, [NotNullWhen(true)] out YamlMappingNode? mappingNode)
    {
        var keyNode = KeyNode(key);
        if (map.Children.TryGetValue(keyNode, out var node) && node is YamlMappingNode x)
        {
            mappingNode = x;
            return true;
        }

        mappingNode = default;
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