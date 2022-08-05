using System.Text.Json;

namespace RendleLabs.OpenApi.Testing.Tests;

public static class JsonAssert
{
    public static void Equivalent(JsonDocument expected, JsonDocument actual)
    {
        foreach (var expectedProperty in expected.RootElement.EnumerateObject())
        {
            var actualProperty = actual.RootElement.GetProperty(expectedProperty.Name);
            Assert.Equal(expectedProperty.Value.ValueKind, actualProperty.ValueKind);
        }
    }
}