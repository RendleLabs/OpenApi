using RendleLabs.OpenApi.Testing;

namespace Testing.Xunit;

public static class XunitAssertExtension
{
    public static void RunAssertions(this OpenApiTest openApiTest)
    {
        var testResponse = openApiTest.TestResponse;
        var actualResponse = openApiTest.ResponseMessage;
    }
}
