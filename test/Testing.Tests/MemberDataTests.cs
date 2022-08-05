using System.Text.Json;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Testing.Tests;

public class MemberDataTests
{
    [Theory]
    [MemberData(nameof(Data))]
    public async Task Run(OpenApiTestRequest testRequest, OpenApiTestResponse expectedTestResponse)
    {
        using var client = new HttpClient();
        var response = await client.ExecuteAsync(testRequest);
        
        Assert.Equal(expectedTestResponse.Status, (int)response.StatusCode);
        if (expectedTestResponse.ContentType is not null)
        {
            Assert.Equal(expectedTestResponse.ContentType, response.Content.Headers.ContentType?.MediaType);
            if (expectedTestResponse.ContentType.StartsWith("text/"))
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(expectedTestResponse.Body, responseBody);
            }
            else if (expectedTestResponse.ContentType == "application/json" && expectedTestResponse.Body is { Length: > 0 })
            {
                var expectedBody = JsonDocument.Parse(expectedTestResponse.Body);
                var actualBody = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                Testing.JsonAssert.Equivalent(expectedBody, actualBody);
            }
        }
    }

    public static IEnumerable<object?[]> Data()
    {
        var testJson = ResourceStreams.Get("HttpBin.yaml.openapi.tests.yaml");
        var apiJson = ResourceStreams.Get("HttpBin.yaml.openapi.yaml");
        var testDocument = new OpenApiTestDocumentParser().Parse(testJson);
        var apiDocument = new OpenApiStreamReader().Read(apiJson, out _);
        var data = new OpenApiTheoryData(testDocument, apiDocument);
        return data.Enumerate();
    }
}