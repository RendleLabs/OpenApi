using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using RendleLabs.OpenApi.Testing.Internal;
using Xunit.Sdk;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestDataAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        throw new NotImplementedException();
    }
}

public class OpenApiTheoryData
{
    private readonly OpenApiTestDocument _testDocument;
    private readonly OpenApiDocument _apiDocument;

    public OpenApiTheoryData(OpenApiTestDocument testDocument, OpenApiDocument apiDocument)
    {
        _testDocument = testDocument;
        _apiDocument = apiDocument;
    }

    public IEnumerable<object?[]> Enumerate()
    {
        var server = _testDocument.Server;

        var list = new List<(OpenApiTestRequest, OpenApiTestResponse)>(_testDocument.Tests.Length);

        foreach (var test in _testDocument.Tests)
        {
            var request = OpenApiTestRequest(server, test.Uri, test.Method, test);

            var response = OpenApiTestResponse(test);

            list.Add((request, response));
        }

        for (int i = list.Count - 1; i >= 0; i++)
        {
            var (request, response) = list[i];
            if (request.Path.Contains('{'))
            {
                var variables = OutputVariables.Get(request.Path);
            }
        }

        throw new NotImplementedException();
    }

    private string CreatePath(OperationType operationType, string pathTemplate, IReadOnlyDictionary<string, string?> parameters)
    {
        if (!_apiDocument.Paths.TryGetValue(pathTemplate, out var pathItem)) return pathTemplate;
        var path = pathTemplate;
        var queryString = new QueryString();
        foreach (var (key, value) in parameters)
        {
            if (pathItem.TryGetParameter(operationType, key, out var parameter))
            {
                switch (parameter.In)
                {
                    case ParameterLocation.Path:
                        path = path.Replace($"{{{key}}}", value);
                        break;
                    case ParameterLocation.Query when value is not null:
                        queryString.Add(key, value);
                        break;
                }
            }
        }

        return path + queryString;
    }

    private static OpenApiTestRequest OpenApiTestRequest(string? server, string path, HttpMethod method, OpenApiTestElement operationTestElement)
    {
        var request = new OpenApiTestRequest
        {
            Server = server,
            Path = path,
            Method = method,
            Body = operationTestElement.RequestBody?.Content,
            ContentType = operationTestElement.RequestBody?.ContentType,
            Headers = operationTestElement.Headers,
        };
        return request;
    }

    private static OpenApiTestResponse OpenApiTestResponse(OpenApiTestElement testElement)
    {
        var response = new OpenApiTestResponse
        {
            Status = testElement.Expect.Status,
            ContentType = testElement.Expect.ResponseBody?.ContentType,
            Body = testElement.Expect.ResponseBody?.Content,
            Headers = testElement.Expect.Headers,
            OutputName = testElement.OutputName,
        };
        return response;
    }

    private static HttpMethod GetHttpMethod(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Get => HttpMethod.Get,
            OperationType.Put => HttpMethod.Put,
            OperationType.Post => HttpMethod.Post,
            OperationType.Delete => HttpMethod.Delete,
            OperationType.Options => HttpMethod.Options,
            OperationType.Head => HttpMethod.Head,
            OperationType.Patch => HttpMethod.Patch,
            OperationType.Trace => HttpMethod.Trace,
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }
}

internal static class OutputVariables
{
    private static readonly Regex TextInBraces = new Regex(@"\{\{.+?\}\}", RegexOptions.Compiled);

    public static OutputVariable[] Get(string input)
    {
        var matches = TextInBraces.Matches(input);
        return matches.Count == 0
            ? Array.Empty<OutputVariable>()
            : matches.Select(m => m.Value.TrimStart('{')).Select(s => s.TrimEnd('}')).Select(Create).ToArray();
    }

    private static OutputVariable Create(string input)
    {
        input = input.TrimStart('{').TrimEnd('}');
        var firstDot = input.IndexOf('.');
        if (firstDot < 1) throw new InvalidOperationException();
        var source = input.Substring(0, firstDot);

        if (source.Equals("header", StringComparison.OrdinalIgnoreCase)) return new HeaderOutputVariable(source, input.Substring(firstDot + 1));

        throw new NotImplementedException();
    }
}

internal abstract class OutputVariable
{
    protected OutputVariable(string source)
    {
        Source = source;
    }

    public string Source { get; }

    public abstract string? Get(HttpResponseMessage response);
}

internal class HeaderOutputVariable : OutputVariable
{
    public HeaderOutputVariable(string source, string name) : base(source)
    {
        Name = name;
    }

    public string Name { get; }

    public override string? Get(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues(Name, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}