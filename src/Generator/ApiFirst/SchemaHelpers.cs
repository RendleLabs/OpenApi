using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

internal static class SchemaHelpers
{
    public static string SchemaTypeToDotNetType(OpenApiSchema schema)
    {
        var type = schema.Type switch
        {
            "boolean" => "bool",
            "number" => "double",
            "string" => StringSchemaType(schema),
            "integer" => SchemaTypeToInteger(schema),
            _ => "object",
        };
        if (schema.Nullable) type += '?';
        return type;
    }

    private static string StringSchemaType(OpenApiSchema schema)
    {
        return schema.Format switch
        {
            "date-time" => "DateTime",
            "time" => "TimeOnly",
            "date" => "DateOnly",
            "duration" => "TimeSpan",
            "uuid" => "Guid",
            "uri" => "Uri",
            _ => "string",
        };
    }

    private static string SchemaTypeToInteger(OpenApiSchema schema)
    {
        return schema.Maximum > int.MaxValue ? "long" : "int";
    }
}