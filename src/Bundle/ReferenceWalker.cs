using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;

namespace RendleLabs.OpenApi.Bundle;

public sealed class ReferenceWalker
{
    public ReferenceWalker()
    {
    }

    public void Walk(OpenApiDocument document, string directory, ReferenceInfoCollection references)
    {
        var referenceVisitor = new ReferenceVisitor(directory, references);
        var walker = new OpenApiWalker(referenceVisitor);
        walker.Walk(document);
    }
}