using System.Diagnostics.CodeAnalysis;
using System.Text;
using SharpYaml.Serialization;

namespace RendleLabs.OpenApi.Bundle;

internal static class YamlMappingNodeExtensions
{
    public static bool TryGetMappingNode(this YamlMappingNode parent, string name, [NotNullWhen(true)] out YamlMappingNode? node)
    {
        foreach (var (key, value) in parent)
        {
            if (key is YamlScalarNode scalarNode && scalarNode.Value == name)
            {
                node = value as YamlMappingNode;
                return node is not null;
            }
        }

        node = null;
        return false;
    }

    public static string ToText(this YamlMappingNode node)
    {
        var document = new YamlDocument(node);
        var builder = new StringBuilder();
        using var writer = new StringWriter(builder);
        var yaml = new YamlStream(document);
        yaml.Save(writer, true);
        return builder.ToString();
    }
}