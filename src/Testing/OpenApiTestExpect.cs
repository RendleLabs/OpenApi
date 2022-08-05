using System.Collections.ObjectModel;

namespace RendleLabs.OpenApi.Testing;

public class OpenApiTestExpect
{
    public OpenApiTestExpect(int status, OpenApiTestBody? responseBody, ReadOnlyDictionary<string, string[]> headers)
    {
        Status = status;
        ResponseBody = responseBody;
        Headers = headers;
    }

    public int Status { get; }
    public OpenApiTestBody? ResponseBody { get; }
    public IReadOnlyDictionary<string, string[]> Headers { get; }
}