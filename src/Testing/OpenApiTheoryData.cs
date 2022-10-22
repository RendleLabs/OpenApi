using Microsoft.OpenApi.Models;
using RendleLabs.OpenApi.Testing.Internal;

namespace RendleLabs.OpenApi.Testing;

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

        foreach (var test in _testDocument.Tests)
        {
            var request = OpenApiTestRequest(server, test.Uri, test.Method, test);

            var response = OpenApiTestResponse(test.Expect);
            
            yield return new object?[] { request, response };
        }
    }

    private string CreatePath(OperationType operationType, string pathTemplate, IReadOnlyDictionary<string,string?> parameters)
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

    private static OpenApiTestRequest OpenApiTestRequest(string? server, string path, HttpMethod method, OpenApiTest operationTest)
    {
        var request = new OpenApiTestRequest
        {
            Server = server,
            Path = path,
            Method = method,
            Body = operationTest.RequestBody?.Content,
            ContentType = operationTest.RequestBody?.ContentType,
            Headers = operationTest.Headers
        };
        return request;
    }

    private static OpenApiTestResponse OpenApiTestResponse(OpenApiTestExpect expect)
    {
        var response = new OpenApiTestResponse
        {
            Status = expect.Status,
            ContentType = expect.ResponseBody?.ContentType,
            Body = expect.ResponseBody?.Content,
            Headers = expect.Headers
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