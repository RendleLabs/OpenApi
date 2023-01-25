using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RendleLabs.OpenApi.Generator.ApiFirst;

namespace RendleLabs.OpenApi.Generator.Tests;

public class ModelFinderTests
{
    private readonly OpenApiDocument _apiDocument;

    public ModelFinderTests()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RendleLabs.OpenApi.Generator.Tests.openapi.yaml");
        _apiDocument = new OpenApiStreamReader().Read(stream, out _);
    }

    [Fact]
    public void FindsModels()
    {
        var actual = ModelFinder.FindModels(_apiDocument).ToArray();
        Assert.NotEmpty(actual);
    }
}