using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Testing.Tests;

public class OpenApiTestDocumentParserTests
{
    [Fact]
    public void ParsesYaml()
    {
        using var stream = ResourceStreams.Get("HttpBin.yaml.openapi.tests.yaml");
        var actual = new OpenApiTestDocumentParser().Parse(stream);
        
        Assert.Equal("https://httpbin.org", actual.Server);
        Assert.NotEmpty(actual.Tests);
        
        var testPath = Assert.Contains("/base64/{value}", actual.Tests);
        var tests = Assert.Contains(OperationType.Get, testPath.Operations);
        
        var test = Assert.Single(tests);
        Assert.Equal("SGVsbG8sIHdvcmxkIQ==", test.Parameters["value"]);
        Assert.Equal(200, test.Expect.Status);
        Assert.Equal("Hello, world!", test.Expect.ResponseBody?.ContentType);

        testPath = Assert.Contains("/todos/{id}", actual.Tests);
        tests = Assert.Contains(OperationType.Get, testPath.Operations);
        
        test = Assert.Single(tests);
        Assert.Equal("1", test.Parameters["id"]);
        Assert.Equal(200, test.Expect.Status);
        var json = Assert.IsType<JsonDocument>(test.Expect.ResponseBody);

        var jsonId = json.RootElement.GetProperty("id").GetInt32();
        var jsonText = json.RootElement.GetProperty("text").GetString();
        var jsonDone = json.RootElement.GetProperty("done").GetBoolean();
        Assert.Equal(1, jsonId);
        Assert.Equal("Write parser", jsonText);
        Assert.True(jsonDone);
    }
}