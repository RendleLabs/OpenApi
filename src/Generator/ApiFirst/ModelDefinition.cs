using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public class ModelDefinition
{
    private readonly Dictionary<string, ModelProperty> _properties = new();
    public ModelDefinition(string openApiName)
    {
        OpenApiName = openApiName;
        CSharpName = CSharpHelpers.ClassName(openApiName);
    }

    public string OpenApiName { get; }
    public string CSharpName { get; }

    public void AddProperty(string name, OpenApiSchema schema)
    {
        var property = new ModelProperty(name, schema);
        _properties[property.CSharpName] = property;
    }

    public ICollection<ModelProperty> Properties => _properties.Values;
}