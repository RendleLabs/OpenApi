using System.Text.Json;

namespace RendleLabs.OpenApi.Testing;

public static class JsonAssert
{
    public static void Equivalent(JsonDocument expected, JsonDocument actual)
    {
        ElementEquivalent(expected.RootElement, actual.RootElement);
    }

    private static void ElementEquivalent(JsonElement expected, JsonElement actual, string jsonPath = "")
    {
        if (expected.ValueKind != actual.ValueKind)
        {
            throw new JsonEqualException(expected.ValueKind, actual.ValueKind, $"{jsonPath}(Kind)");
        }
        
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                ObjectEquivalent(expected, actual, jsonPath);
                break;
            case JsonValueKind.Array:
                ArrayEquivalent(expected, actual, jsonPath);
                break;
            case JsonValueKind.String:
                StringEquivalent(expected.GetString(), actual.GetString(), jsonPath);
                break;
            case JsonValueKind.Number:
                NumberEquivalent(expected.GetDecimal(), actual.GetDecimal(), jsonPath);
                break;
            case JsonValueKind.Undefined:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void StringEquivalent(string? expected, string? actual, string jsonPath)
    {
        if (expected != actual)
        {
            throw new JsonEqualException(expected, actual, jsonPath);
        }
    }

    private static void NumberEquivalent(decimal? expected, decimal? actual, string jsonPath)
    {
        if (expected != actual)
        {
            throw new JsonEqualException(expected, actual, jsonPath);
        }
    }

    private static void ObjectEquivalent(JsonElement expected, JsonElement actual, string? jsonPath)
    {
        foreach (var expectedProperty in expected.EnumerateObject())
        {
            var actualProperty = actual.GetProperty(expectedProperty.Name);
            ElementEquivalent(expectedProperty.Value, actualProperty, $"{jsonPath}['{expectedProperty.Name}']");
        }
    }

    private static void ArrayEquivalent(JsonElement expected, JsonElement actual, string jsonPath)
    {
        var expectedArray = expected.EnumerateArray().ToArray();
        var actualArray = actual.EnumerateArray().ToArray();

        if (expectedArray.Length != actualArray.Length)
        {
            throw new JsonEqualException(expectedArray.Length, actualArray.Length, $"{jsonPath}[](Length)");
        }

        for (int i = 0, l = expectedArray.Length; i < l; i++)
        {
            ElementEquivalent(expectedArray[i], actualArray[i], $"{jsonPath}[{i}]");
        }
    }
}