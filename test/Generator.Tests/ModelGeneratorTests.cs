using System.Reflection;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RendleLabs.OpenApi.Generator.ApiFirst;

namespace RendleLabs.OpenApi.Generator.Tests;

public class ModelGeneratorTests
{
    private readonly OpenApiDocument _apiDocument;

    public ModelGeneratorTests()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RendleLabs.OpenApi.Generator.Tests.openapi.yaml");
        _apiDocument = new OpenApiStreamReader().Read(stream, out _);
    }
    
    [Fact]
    public async Task GeneratesOneOfEachClass()
    {
        var builder = new StringBuilder();
        var writer = new StringWriter(builder);

        var target = new ModelGenerator();
        foreach (var schema in ModelFinder.FindModels(_apiDocument))
        {
            target.AddSchema(schema);
        }
        
        await target.GenerateAsync(writer);
        await writer.FlushAsync();
        var actual = builder.ToString().Trim();
        Assert.Equal(ExpectedClasses.Trim(), actual);
    }

    private const string ExpectedClasses = @"
public partial class Book
{
    [global::System.Text.Json.Serialization.JsonPropertyName(""id"")]
    public int? Id { get; set; }
    [global::System.Text.Json.Serialization.JsonPropertyName(""title"")]
    public string? Title { get; set; }
    [global::System.Text.Json.Serialization.JsonPropertyName(""author"")]
    public string? Author { get; set; }
}

public partial class NewBook
{
    [global::System.Text.Json.Serialization.JsonPropertyName(""title"")]
    public string? Title { get; set; }
    [global::System.Text.Json.Serialization.JsonPropertyName(""author"")]
    public string? Author { get; set; }
}
";
}