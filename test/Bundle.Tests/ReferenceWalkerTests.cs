using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;

namespace RendleLabs.OpenApi.Bundle.Tests;

public class ReferenceWalkerTests
{
    [Fact]
    public async Task FindsReferencesInYaml()
    {
        var path = GetPath("openapi.yaml");
        var directory = Path.GetDirectoryName(path)!;
        var document = await LoadOpenApiDocument(path)!;
        if (document is null) throw new InvalidOperationException();
        
        var walker = new ReferenceWalker();
        var references = new ReferenceInfoCollection();
        walker.Walk(document, directory, references);

        await new ReferenceResolver(document, references).ResolveAsync();
        
        Assert.NotNull(document);

        var country = Assert.Contains("Country", document.Components.Schemas);
        var isoCountryCode = Assert.Contains("IsoCountryCode", document.Components.Schemas);
        var internalServerError = Assert.Contains("InternalServerError", document.Components.Responses);
    }

    private static async Task<OpenApiDocument?> LoadOpenApiDocument(string openApiPath)
    {
        var stream = File.OpenRead(openApiPath);
        var result = await new OpenApiStreamReader().ReadAsync(stream);
        return result.OpenApiDocument;
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