using Xunit.Sdk;

namespace RendleLabs.OpenApi.Testing;

public class JsonEqualException : AssertActualExpectedException
{
    public JsonEqualException(object? expected, object? actual, string jsonPath)
        : base(expected, actual, CreateMessage(jsonPath), null, null)
    {
    }

    public JsonEqualException(object? expected, object? actual, string jsonPath, Exception? innerException)
        : base(expected, actual, CreateMessage(jsonPath), null, null, innerException)
    {
    }

    private static string CreateMessage(string jsonPath) => $"Failure: JsonEqual {jsonPath}";
}