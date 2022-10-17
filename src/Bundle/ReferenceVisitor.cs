using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Path = System.IO.Path;

namespace RendleLabs.OpenApi.Bundle;

public class ReferenceVisitor : OpenApiVisitorBase
{
    private readonly OpenApiDocument _document;
    private readonly string _baseDirectory;
    private readonly SchemaLoader _schemaLoader;

    public bool AnyChanges { get; set; }
    public Dictionary<string, string> PathToIdLookup { get; } = new(StringComparer.OrdinalIgnoreCase);

    public ReferenceVisitor(OpenApiDocument document, string baseDirectory)
    {
        _schemaLoader = new SchemaLoader();
        _document = document;
        _baseDirectory = baseDirectory;
    }

    public override void Visit(IOpenApiReferenceable referenceable)
    {
        if (referenceable is not OpenApiSchema) return;
        if (!referenceable.UnresolvedReference) return;
        if (!referenceable.Reference.IsExternal) return;
        if (referenceable.Reference.ExternalResource is not { Length: > 0 } externalResource) return;

        var path = Path.GetFullPath(externalResource, _baseDirectory);

        if (!PathToIdLookup.TryGetValue(path, out var id))
        {
            var schema = _schemaLoader.LoadSchema(path, out var diagnostic);
            if (schema is null) return;

            AnyChanges = true;
            
            id = GetComponentId(path);

            PathToIdLookup[path] = id;

            _document.Components ??= new OpenApiComponents();
            _document.Components.Schemas[id] = schema;
        }

        referenceable.Reference = new OpenApiReference
        {
            Id = id,
            Type = ReferenceType.Schema,
            HostDocument = _document,
        };
        referenceable.UnresolvedReference = false;
    }
    
    private static string GetComponentId(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path.AsSpan()).TrimStart('.');
        int dot = fileName.IndexOf('.');
        if (dot > -1)
        {
            fileName = fileName[..dot];
        }

        return new string(fileName);
    }
}