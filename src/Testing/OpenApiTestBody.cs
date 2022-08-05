namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestBody
{
    public OpenApiTestBody(string contentType, string requestBody)
    {
        ContentType = contentType;
        Content = requestBody;
    }

    public string ContentType { get; }
    public string Content { get; }
}