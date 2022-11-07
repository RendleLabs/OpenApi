using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;

namespace RendleLabs.OpenApi.Bundle;

public sealed class Bundler : IDisposable
{
    private readonly Stream _source;
    private readonly string _directory;

    public Bundler(Stream source, string filePath)
    {
        _source = source;
        _directory = Path.GetDirectoryName(filePath)!;
    }

    public async Task<OpenApiDocument?> Build()
    {
        var result = await new OpenApiStreamReader().ReadAsync(_source);
        
        result.OpenApiDiagnostic.Write();

        if (result.OpenApiDiagnostic.Errors is { Count: > 0 } errors)
        {
            return null;
        }

        var document = result.OpenApiDocument;

        var references = new ReferenceInfoCollection();

        var referenceVisitor = new ReferenceVisitor(_directory, references);
        var walker = new OpenApiWalker(referenceVisitor);
        walker.Walk(document);
        if (references.Count == 0) return document;

        return document;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}