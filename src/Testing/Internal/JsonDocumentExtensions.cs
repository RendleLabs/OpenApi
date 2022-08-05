using System.Buffers;
using System.Text;
using System.Text.Json;

namespace RendleLabs.OpenApi.Testing.Internal;

public static class JsonDocumentExtensions
{
    public static string? ToUtf8String(this JsonDocument? jsonDocument)
    {
        if (jsonDocument is null) return null;
        var bufferWriter = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(bufferWriter);
        jsonDocument.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
    }
}