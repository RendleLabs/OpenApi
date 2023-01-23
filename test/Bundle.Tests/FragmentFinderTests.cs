using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Bundle.Tests;

public class FragmentFinderTests
{
    [Fact]
    public void FindsSchema()
    {
        var yaml = @"components:
  schemas:
    foo:
      type: object
      properties:
        id:
          type: number";

        var schema = FragmentFinder.Find<OpenApiSchema>(yaml, "components/schemas/foo");
        Assert.Equal("object", schema.Type);
    }
}