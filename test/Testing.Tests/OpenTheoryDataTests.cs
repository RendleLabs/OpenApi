using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Testing.Tests;


public class OpenTheoryDataTests
{
    [Fact]
    public void EnumerateTests()
    {
        var testYaml = ResourceStreams.Get("HttpBin.yaml.openapi.tests.yaml");
        var apiYaml = ResourceStreams.Get("HttpBin.yaml.openapi.yaml");

        var testDocument = new OpenApiTestDocumentParser().Parse(testYaml);
        var apiDocument = new OpenApiStreamReader().Read(apiYaml, out _);

        var target = new OpenApiTheoryData(testDocument, apiDocument);
        var actual = target.Enumerate().First();
        Assert.Equal(2, actual.Length);
        var request = Assert.IsType<OpenApiTestRequest>(actual[0]);
        Assert.Equal("/base64/SGVsbG8sIHdvcmxkIQ==", request.Path);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Null(request.Body);
        var response = Assert.IsType<OpenApiTestResponse>(actual[1]);
        Assert.Equal(200, response.Status);
        Assert.Equal("Hello, world!", response.Body);
    }
}