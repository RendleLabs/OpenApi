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

        var test = actual.Tests[0];
        
        Assert.Equal("/base64/SGVsbG8sIHdvcmxkIQ==", test.Uri);
        Assert.Equal(HttpMethod.Get, test.Method);
        
        Assert.Equal(200, test.Expect.Status);
        Assert.Equal("Hello, world!", test.Expect.ResponseBody?.Content);

        test = actual.Tests[1];

        Assert.Equal("/anything/foo", test.Uri);
        Assert.Equal(HttpMethod.Post, test.Method);
        Assert.Equal("application/json", test.RequestBody?.ContentType);

        var json = JsonDocument.Parse(test.RequestBody!.Content);
        Assert.True(json.RootElement.TryGetProperty("id", out var id));
        Assert.Equal("bar", id.GetString());
        Assert.Equal(201, test.Expect.Status);
        
        Assert.Equal("application/json", test.Expect.ResponseBody?.ContentType);
        json = JsonDocument.Parse(test.Expect.ResponseBody!.Content);
        Assert.True(json.RootElement.TryGetProperty("json", out var jsonElement));
        Assert.True(jsonElement.TryGetProperty("id", out id));
        Assert.Equal("bar", id.GetString());
        Assert.True(json.RootElement.TryGetProperty("url", out var urlElement));
        Assert.Equal("https://httpbin.org/anything/foo", urlElement.GetString());
    }
}