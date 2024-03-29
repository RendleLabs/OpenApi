﻿using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;

namespace Build;

public sealed class Builder : IDisposable
{
    private readonly Stream _source;
    private readonly string _filePath;
    private readonly string _directory;

    public Builder(Stream source, string filePath)
    {
        _source = source;
        _filePath = filePath;
        _directory = Path.GetDirectoryName(filePath)!;
    }

    public async Task<OpenApiDocument?> Build()
    {
        // var settings = new OpenApiReaderSettings
        // {
        //     LoadExternalRefs = true,
        //     BaseUrl = new Uri(_filePath),
        // };
        
        var result = await new OpenApiStreamReader().ReadAsync(_source);
        
        result.OpenApiDiagnostic.Write();

        if (result.OpenApiDiagnostic.Errors is { Count: > 0 } errors)
        {
            return null;
        }

        var referenceVisitor = new ReferenceVisitor(result.OpenApiDocument, _directory);
        var walker = new OpenApiWalker(referenceVisitor);
        
        do
        {
            referenceVisitor.AnyChanges = false;
            walker.Walk(result.OpenApiDocument);
        } while (referenceVisitor.AnyChanges);

        return result.OpenApiDocument;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}