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

        var tests = new List<OpenApiTestElement>();
        
        if (node.TryGetSequence("tests", out var testsNode))
        {
            foreach (var testNode in testsNode.OfType<YamlMappingNode>())
            {
                foreach (var (pathNode, map) in testNode.ChildrenOfType<YamlScalarNode, YamlMappingNode>())
                {
                    if (TryParseTestNode(pathNode.Value, map, out var test))
                    {
                        tests.Add(test);
                    }
                }
            }
        }

        return new OpenApiTestDocument(server, tests);
    }

    private static bool TryParseTestNode(string? key, YamlMappingNode value, [NotNullWhen(true)] out OpenApiTestElement? test)
    {
        test = null;
        if (key is not { Length: > 0 }) return false;
        if (!TryParseMethodAndPath(key, out var method, out var path)) return false;
        if (!TryParseTestNode(value, out test)) return false;
        test.Uri = path;
        test.Method = method;

        return true;
    }

    private static bool TryParseMethodAndPath(string key, [NotNullWhen(true)] out HttpMethod? method, [NotNullWhen(true)] out string? path)
    {
        path = null;
        method = null;
        var parts = key.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            if (parts[1] is not { Length: > 0 }) return false;
            if (!TryParseMethod(parts[0], out method)) return false;
            path = parts[1];
            return true;
        }

        return false;
    }

    private static bool TryParseMethod(string str, [NotNullWhen(true)] out HttpMethod? method)
    {
        method = str.ToUpperInvariant() switch
        {
            "DELETE" => HttpMethod.Delete,
            "GET" => HttpMethod.Get,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            "PATCH" => HttpMethod.Patch,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "TRACE" => HttpMethod.Trace,
            _ => null
        };

        return method is not null;
    }

    private static bool TryParsePathNode(YamlNode key, YamlNode value, [NotNullWhen(true)] out HttpMethod? method, [NotNullWhen(true)] out OpenApiTestPath? path)
    {
        method = null;
        path = null;
        if (key is not YamlScalarNode keyNode || keyNode.Value is null) return false;
        if (value is not YamlMappingNode valueMap) return false;

        var operations = new Dictionary<OperationType, OpenApiTestElement[]>();

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

    private static bool TryParseOperationNode(YamlNode value, [NotNullWhen(true)] out OpenApiTestElement[]? tests)
    {
        tests = null;
        if (value is not YamlSequenceNode nodes) return false;

        tests = ParseTestNodes(nodes).ToArray();
        return true;
    }

    private static IEnumerable<OpenApiTestElement> ParseTestNodes(YamlSequenceNode nodes)
    {
        foreach (var child in nodes.Children)
        {
            if (TryParseTestNode(child, out var test))
            {
                yield return test;
            }
        }
    }

    private static bool TryParseTestNode(YamlNode node, [NotNullWhen(true)] out OpenApiTestElement? test)
    {
        test = null;
        if (node is not YamlMappingNode map) return false;
        var headers = ParseHeaders(map);

        var requestBody = ParseBody(map, "requestBody");
        
        var expect = ParseExpect(map);

        test = new OpenApiTestElement
        {
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