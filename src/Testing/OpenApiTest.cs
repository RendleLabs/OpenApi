using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTest
{
    public OpenApiTest(OpenApiTestRequest testRequest, OpenApiTestResponse testResponse)
    {
        TestRequest = testRequest;
        TestResponse = testResponse;
    }

    internal OpenApiTestRequest TestRequest { get; }
    internal OpenApiTestResponse TestResponse { get; }
    internal HttpRequestMessage RequestMessage { get; private set; }
    internal HttpResponseMessage ResponseMessage { get; private set; }
    
    public async Task ExecuteAsync(HttpClient client)
    {
        
        var url = TestRequest.ToUri();
        RequestMessage = new HttpRequestMessage(TestRequest.Method, url);
        
        if (TestRequest.Headers is { Count: > 0 })
        {
            foreach (var (key, values) in TestRequest.Headers)
            {
                RequestMessage.Headers.TryAddWithoutValidation(key, values);
            }
        }
        
        if (TestRequest.Body != null)
        {
            RequestMessage.Content = new StringContent(TestRequest.Body, Encoding.UTF8, TestRequest.ContentType);
        }
        
        ResponseMessage = await client.SendAsync(RequestMessage);
    }

    public async Task Assert()
    {
        ((int)ResponseMessage.StatusCode).Should().Be(TestResponse.Status);

        if (TestResponse.Headers is { Count: > 0 } expectedHeaders)
        {
            foreach (var (expectedKey, expectedValues) in expectedHeaders)
            {
                ResponseMessage.Headers.Should().ContainKey(expectedKey);
                var actualValues = ResponseMessage.Headers.GetValues(expectedKey)!.ToHashSet();
                foreach (var expectedValue in expectedValues)
                {
                    if (expectedValue.StartsWith('/') && expectedValue.EndsWith('/'))
                    {
                        var regex = new Regex(expectedValue.Trim('/'));
                        actualValues.Should().Contain(s => regex.IsMatch(s));
                    }
                    else
                    {
                        actualValues.Should().Contain(expectedValue);
                    }
                }
            }
        }

        if (TestResponse.ContentType is { Length: > 0 } contentType)
        {
            contentType.Should().Be(ResponseMessage.Content.Headers.ContentType?.ToString());

            if (TestResponse.Body is { Length: > 0 } expectedBody)
            {
                if (contentType.StartsWith("text/"))
                {
                    var body = await ResponseMessage.Content.ReadAsStringAsync();
                    body.Trim().Should().Be(expectedBody.Trim());
                }
                else if (contentType == "application/json")
                {
                    var expectedJson = JsonDocument.Parse(expectedBody);
                    var actualJson = await JsonDocument.ParseAsync(await ResponseMessage.Content.ReadAsStreamAsync());
                    JsonAssert.Equivalent(expectedJson, actualJson);
                }
            }
        }
    }
}