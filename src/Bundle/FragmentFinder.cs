using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Readers;
using SharpYaml.Serialization;

namespace RendleLabs.OpenApi.Bundle;

internal static class FragmentFinder
{
    public static T Find<T>(string source, string path) where T : IOpenApiElement
    {
        int hash = path.IndexOf('#');
        if (hash > 0) path = path.Substring(hash).Trim('#', '/');
        var parts = path.Split('/');
        
        var yaml = ParseYaml(source);

        YamlMappingNode? node = null;
        
        foreach (var document in yaml.Documents)
        {
            node = (YamlMappingNode)document.RootNode;
            foreach (var part in parts)
            {
                if (!node.TryGetMappingNode(part, out node))
                {
                    break;
                }
            }

            if (node is not null) break;
        }

        var fragment = node?.ToText();

        var reader = new OpenApiStringReader();
        var element = reader.ReadFragment<T>(fragment, OpenApiSpecVersion.OpenApi3_0, out var diagnostic);

        if (diagnostic.Errors is { Count: > 0 })
        {
            if (diagnostic.Errors.Any(e => !e.Message.Contains("is not a valid property")))
            {
                throw new BundleException("Error reading fragment", diagnostic);
            }
        }

        return element;
    }

    private static YamlStream ParseYaml(string source)
    {
        var yaml = new YamlStream();
        using var stringReader = new StringReader(source);
        yaml.Load(stringReader);

        return yaml;
    }
}