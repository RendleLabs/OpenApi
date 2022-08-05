using System.Text.Json;
using Xunit;

namespace RendleLabs.OpenApi.Testing;

public static class JsonAssert
{
    public static void Equivalent(JsonDocument expected, JsonDocument actual)
    {
        ElementEquivalent(expected.RootElement, actual.RootElement);
    }

    private static void ElementEquivalent(JsonElement expected, JsonElement actual)
    {
        Assert.Equal(expected.ValueKind, actual.ValueKind);

        switch (actual.ValueKind)
        {
            case JsonValueKind.Object:
                ObjectEquivalent(expected, actual);
                break;
            case JsonValueKind.Array:
                ArrayEquivalent(expected, actual);
                break;
            case JsonValueKind.String:
                Assert.Equal(expected.GetString(), actual.GetString());
                break;
            case JsonValueKind.Number:
                Assert.Equal(expected.GetDouble(), actual.GetDouble());
                break;
        }
    }

    private static void ArrayEquivalent(JsonElement expected, JsonElement actual)
    {
        var expectedElements = expected.EnumerateArray().ToArray();
        var actualElements = actual.EnumerateArray().ToArray();
        Assert.Equal(expectedElements.Length, actualElements.Length);

        for (int i = 0, l = expectedElements.Length; i < l; i++)
        {
            ElementEquivalent(expectedElements[i], actualElements[i]);
        }
    }
    
    private static void ObjectEquivalent(JsonElement expected, JsonElement actual)
    {
        foreach (var expectedProperty in expected.EnumerateObject())
        {
            Assert.True(actual.TryGetProperty(expectedProperty.Name, out var actualProperty));
            ElementEquivalent(expectedProperty.Value, actualProperty);
        }
    }
}