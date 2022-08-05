using System.Text.Json;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace RendleLabs.OpenApi.Testing.Internal;

internal static class YamlToJson
{
    public static JsonDocument? ToJson(YamlMappingNode map)
    {
        using var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream);
        
        WriteMap(writer, map);
        
        writer.Flush();

        stream.Position = 0;
        return JsonDocument.Parse(stream);
    }

    private static void WriteMap(Utf8JsonWriter writer, YamlMappingNode map)
    {
        writer.WriteStartObject();
    
        foreach (var (k, v) in map)
        {
            if (k is not YamlScalarNode keyNode || keyNode.Value is not { Length: > 0 }) continue;
    
            writer.WritePropertyName(keyNode.Value);
            
            switch (v)
            {
                case YamlScalarNode scalarNodeValue:
                    WriteScalarValue(writer, scalarNodeValue);
                    break;
                case YamlMappingNode mapNodeValue:
                    WriteMap(writer, mapNodeValue);
                    break;
                case YamlSequenceNode sequenceNodeValue:
                    WriteArray(writer, sequenceNodeValue);
                    break;
            }
        }
        
        writer.WriteEndObject();
    }

    private static void WriteArray(Utf8JsonWriter writer, YamlSequenceNode sequence)
    {
        writer.WriteStartArray();
        foreach (var item in sequence)
        {
            switch (item)
            {
                case YamlScalarNode scalar:
                    WriteScalarValue(writer, scalar);
                    break;
                case YamlMappingNode map:
                    WriteMap(writer, map);
                    break;
                case YamlSequenceNode seq:
                    WriteArray(writer, seq);
                    break;
            }
        }
        writer.WriteEndArray();
    }
    
    private static void WriteScalarValue(Utf8JsonWriter writer, YamlScalarNode value)
    {
        if (value.Value is null) writer.WriteNullValue();

        if (value.Style == ScalarStyle.Plain)
        {
            switch (value.Value)
            {
                case "true":
                    writer.WriteBooleanValue(true);
                    return;
                case "false":
                    writer.WriteBooleanValue(false);
                    return;
                case "null":
                    writer.WriteNullValue();
                    return;
            }

            if (long.TryParse(value.Value, out var longValue))
            {
                writer.WriteNumberValue(longValue);
                return;
            }

            if (double.TryParse(value.Value, out var doubleValue))
            {
                writer.WriteNumberValue(doubleValue);
                return;
            }
        }
        
        writer.WriteStringValue(value.Value);
    }
    
}