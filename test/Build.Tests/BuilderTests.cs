using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Writers;

namespace Build.Tests;

public class BuilderTests
{
    [Fact]
    public async Task BuildsCountryDocument()
    {
        var directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "TestData");
        var openapiPath = Path.Combine(directory, "openapi.json");
        var stream = File.OpenRead(openapiPath);
        var builder = new Builder(stream, openapiPath);
        var document = await builder.Build();
        Assert.NotNull(document);
        var settings = new OpenApiWriterSettings
        {
            InlineLocalReferences = false
        };
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        var writer = new OpenApiYamlWriter(sw, settings);
        // writer.wr
        document.Serialize(writer, OpenApiSpecVersion.OpenApi3_0);
        var yaml = sb.ToString();
        Assert.NotEmpty(yaml);
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