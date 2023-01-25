using System.CodeDom.Compiler;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Generator.ApiFirst;

public class ModelGenerator
{
    private readonly List<OpenApiSchema> _openApiSchemata = new();
    
    public void AddSchema(OpenApiSchema openApiSchema)
    {
        _openApiSchemata.Add(openApiSchema);
    }

    public async Task GenerateAsync(TextWriter writer)
    {
        var indentedTextWriter = writer as IndentedTextWriter ?? new IndentedTextWriter(writer);
        
        var definitions = CreateModelDefinitions();
        foreach (var definition in definitions.OrderBy(d => d.CSharpName))
        {
            await Generate(indentedTextWriter, definition);
        }

        await indentedTextWriter.FlushAsync();
    }

    private static async Task Generate(IndentedTextWriter writer, ModelDefinition definition)
    {
        await writer.WriteLineAsync($"public partial class {definition.CSharpName}");
        
        using (writer.OpenBrace())
        {
            foreach (var property in definition.Properties)
            {
                await writer.WriteLineAsync($"[global::System.Text.Json.Serialization.JsonPropertyName(\"{property.OpenApiName}\")]");
                await writer.WriteLineAsync($"public {property.CSharpType}? {property.CSharpName} {{ get; set; }}");
            }
        }

        await writer.WriteLineNoTabsAsync();
    }

    private ModelDefinition[] CreateModelDefinitions()
    {
        var definitions = new Dictionary<string, ModelDefinition>();
        
        foreach (var openApiSchema in _openApiSchemata)
        {
            if (!definitions.TryGetValue(openApiSchema.Title, out var definition))
            {
                definitions[openApiSchema.Title] = definition = new ModelDefinition(openApiSchema.Title);
            }

            foreach (var (name, schema) in openApiSchema.Properties)
            {
                definition.AddProperty(name, schema);
            }
        }

        return definitions.Values.ToArray();
    }
}