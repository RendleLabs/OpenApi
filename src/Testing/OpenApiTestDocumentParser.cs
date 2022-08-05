using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;
using RendleLabs.OpenApi.Testing.Internal;
using YamlDotNet.RepresentationModel;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestDocumentParser
{
    public OpenApiTestDocument Parse(Stream stream)
    {
        var yamlStream = new YamlStream();
        
        using (var reader = new StreamReader(stream))
        {
            yamlStream.Load(reader);
        }

        var document = yamlStream.Documents.FirstOrDefault();
        if (document is null) throw new InvalidOperationException();

        if (document.RootNode is not YamlMappingNode node) throw new InvalidOperationException();

        var server = node.TryGetScalar("server", out var serverNode)
            ? serverNode.Value
            : null;

        var tests = new Dictionary<string, OpenApiTestPath>();
        
        if (node.TryGetMap("tests", out var testsNode))
        {
            foreach (var (k, v) in testsNode.Children)
            {
                if (TryParsePathNode(k, v, out var path))
                {
                    tests.Add(path.Path, path);
                }
            }
        }

        return new OpenApiTestDocument(server, tests);
    }

    private static bool TryParsePathNode(YamlNode key, YamlNode value, [NotNullWhen(true)] out OpenApiTestPath? path)
    {
        path = null;
        if (key is not YamlScalarNode keyNode || keyNode.Value is null) return false;
        if (value is not YamlMappingNode valueMap) return false;

        var operations = new Dictionary<OperationType, OpenApiTest[]>();

        foreach (var (k, v) in valueMap.Children)
        {
            if (k is YamlScalarNode operationKey && Enum.TryParse(operationKey.Value, true, out OperationType operationType))
            {
                if (TryParseOperationNode(v, out var tests))
                {
                    operations[operationType] = tests;
                }
            }
        }

        path = new OpenApiTestPath(keyNode.Value, operations);

        return true;
    }

    private static bool TryParseOperationNode(YamlNode value, [NotNullWhen(true)] out OpenApiTest[]? tests)
    {
        tests = null;
        if (value is not YamlSequenceNode nodes) return false;

        tests = ParseTestNodes(nodes).ToArray();
        return true;
    }

    private static IEnumerable<OpenApiTest> ParseTestNodes(YamlSequenceNode nodes)
    {
        foreach (var child in nodes.Children)
        {
            if (TryParseTestNode(child, out var test))
            {
                yield return test;
            }
        }
    }

    private static bool TryParseTestNode(YamlNode node, [NotNullWhen(true)] out OpenApiTest? test)
    {
        test = null;
        if (node is not YamlMappingNode map) return false;

        var parameters = ParseParameters(map);
        var headers = ParseHeaders(map);

        var requestBody = ParseBody(map, "requestBody");
        
        var expect = ParseExpect(map);

        test = new OpenApiTest
        {
            Parameters = new ReadOnlyDictionary<string, string?>(parameters),
            Expect = expect,
            RequestBody = requestBody,
            Headers = headers,
        };

        return true;
    }

    private static OpenApiTestBody? ParseBody(YamlMappingNode map, string name)
    {
        if (!map.TryGetMap(name, out var bodyMap)) return null;
        var (keyNode, bodyNode) = bodyMap.First();
        
        var contentType = ((YamlScalarNode)keyNode).Value;
        var requestBody = bodyNode switch
        {
            YamlScalarNode requestBodyScalar => requestBodyScalar.Value,
            YamlMappingNode requestBodyMap => YamlToJson.ToJson(requestBodyMap)?.ToUtf8String(),
            _ => null
        };

        return requestBody is not null
            ? new OpenApiTestBody(contentType, requestBody)
            : null;
    }

    private static OpenApiTestExpect ParseExpect(YamlMappingNode map)
    {
        if (!map.TryGetMap("expect", out var expectMap))
        {
            throw new InvalidOperationException();
        }

        expectMap.TryGetInt("status", out int status);
        var responseBody = ParseBody(expectMap, "responseBody");
        var headers = ParseHeaders(expectMap);

        return new OpenApiTestExpect(status, responseBody, headers);
    }

    private static object? TryParseBody(YamlMappingNode node, string key)
    {
        if (!node.TryGetNode("responseBody", out var responseBodyNode)) return null;
        
        return responseBodyNode switch
        {
            YamlScalarNode scalarNode => scalarNode.Value,
            YamlMappingNode mappingNode => YamlToJson.ToJson(mappingNode),
            _ => null
        };

    }
    
    private static ReadOnlyDictionary<string, string?> ParseParameters(YamlMappingNode map)
    {
        var dictionary = new Dictionary<string, string?>();
        
        if (map.TryGetMap("parameters", out var parameterMap))
        {
            foreach (var (k, v) in parameterMap)
            {
                if (k is YamlScalarNode keyNode && v is YamlScalarNode valueNode)
                {
                    if (keyNode.Value is not null)
                    {
                        dictionary.Add(keyNode.Value, valueNode.Value);
                    }
                }
            }
        }

        return new ReadOnlyDictionary<string, string?>(dictionary);
    }

    private static ReadOnlyDictionary<string, string[]> ParseHeaders(YamlMappingNode map)
    {
        var dictionary = new Dictionary<string, string[]>();
        
        if (map.TryGetMap("headers", out var parameterMap))
        {
            foreach (var (k, v) in parameterMap)
            {
                if (k is YamlScalarNode keyNode && keyNode.Value is not null)
                {
                    var values = v switch
                    {
                        YamlScalarNode scalarValue when scalarValue.Value is not null => new[] { scalarValue.Value },
                        YamlSequenceNode sequenceValue => sequenceValue.ToStringArray(),
                        _ => Array.Empty<string>()
                    };
                    dictionary[keyNode.Value] = values;
                }
            }
        }

        return new ReadOnlyDictionary<string, string[]>(dictionary);
    }

}

internal static class YamlSequenceNodeExtensions
{
    public static string[] ToStringArray(this YamlSequenceNode node)
    {
        return node.Children.OfType<YamlScalarNode>()
            .Where(i => i.Value is not null)
            .Select(i => i.Value!)
            .ToArray();
    }
}