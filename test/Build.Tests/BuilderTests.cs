using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Build.Tests;

public class BuilderTests
{
    [Fact]
    public async Task BuildsCountryDocumentFromJson()
    {
        var document = await LoadOpenApiDocument("openapi.json");
        Assert.NotNull(document);
        var path = document!.Paths["/api/countries/{isoCountryCode}"];
        var parameter = path.Parameters.Single();
        Assert.Equal("#/components/schemas/IsoCountryCode", parameter.Schema.Reference.ReferenceV3);
        Assert.True(document.Components.Schemas.TryGetValue("IsoCountryCode", out var schema));
        Assert.Equal("string", schema!.Type);
    }

    [Fact]
    public async Task BuildsCountryDocumentFromYaml()
    {
        var document = await LoadOpenApiDocument("openapi.yaml");
        Assert.NotNull(document);
        var path = document!.Paths["/api/countries/{isoCountryCode}"];
        var parameter = path.Parameters.Single();
        Assert.Equal("#/components/schemas/IsoCountryCode", parameter.Schema.Reference.ReferenceV3);
        Assert.True(document.Components.Schemas.TryGetValue("IsoCountryCode", out var schema));
        Assert.Equal("string", schema!.Type);
    }

    private static async Task<OpenApiDocument?> LoadOpenApiDocument(string fileName)
    {
        var openapiPath = GetPath(fileName);
        var stream = File.OpenRead(openapiPath);
        var builder = new Builder(stream, openapiPath);
        var document = await builder.Build();
        return document;
    }

    private static string GetPath(string fileName)
    {
        var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestData");
        var openapiPath = Path.Combine(directory, fileName);
        return openapiPath;
    }

    private static string ToString(OpenApiDocument document)
    {
        var settings = new OpenApiWriterSettings
        {
            InlineLocalReferences = false,
        };
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        var writer = new OpenApiYamlWriter(sw, settings);
        document.Serialize(writer, OpenApiSpecVersion.OpenApi3_0);
        return sb.ToString();
    }
}

public class SchemaLoaderTests
{
    [Fact]
    public void LoadsSchema()
    {
        var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestData");
        var schemaPath = Path.Combine(directory, "country.schema.json");
        var loader = new SchemaLoader();
        var schema = loader.LoadSchema(schemaPath, out var diagnostic);
        Assert.NotNull(schema);
    }
}