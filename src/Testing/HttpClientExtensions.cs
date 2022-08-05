using System.Text;

namespace RendleLabs.OpenApi.Testing;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> ExecuteAsync(this HttpClient client, OpenApiTestRequest testRequest)
    {
        var url = testRequest.ToUri();
        var message = new HttpRequestMessage(testRequest.Method, url);
        
        if (testRequest.Headers is { Count: > 0 })
        {
            foreach (var (key, values) in testRequest.Headers)
            {
                message.Headers.TryAddWithoutValidation(key, values);
            }
        }
        
        if (testRequest.Body != null)
        {
            message.Content = new StringContent(testRequest.Body, Encoding.UTF8, testRequest.ContentType);
        }
        
        return client.SendAsync(message);
    }
}