using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public class ModelProperty
{
    public ModelProperty(string openApiName, OpenApiSchema schema)
    {
        OpenApiName = openApiName;
        OpenApiType = schema.Type;
        CSharpName = CSharpHelpers.PropertyName(openApiName);
        CSharpType = SchemaHelpers.SchemaTypeToDotNetType(schema);
    }

    public string OpenApiName { get; }
    public string OpenApiType { get; }
    public string CSharpName { get; }
    public string CSharpType { get; }
}