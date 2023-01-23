using System.Text.RegularExpressions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Path = System.IO.Path;

namespace RendleLabs.OpenApi.Bundle;

public class ReferenceVisitor : OpenApiVisitorBase
{

    private readonly string _basePath;
    private readonly ReferenceInfoCollection _references;

    public bool AnyChanges { get; set; }
    public Dictionary<string, string> PathToIdLookup { get; } = new(StringComparer.OrdinalIgnoreCase);

    public ReferenceVisitor(string basePath, ReferenceInfoCollection references)
    {
        _basePath = basePath;
        _references = references;
    }

    private void VisitReference<T>(T element) where T : IOpenApiReferenceable
    {
        if (!element.UnresolvedReference) return;
        if (!element.Reference.IsExternal) return;
        if (element.Reference.ExternalResource is not { Length: > 0 } externalResource) return;

        if (ReferencePath.IsHttp(_basePath))
        {
            if (ReferencePath.IsHttp(externalResource))
            {
                element.Reference.ExternalResource = externalResource;
            }
            else
            {
                var baseUri = new Uri(_basePath);
                element.Reference.ExternalResource = new Uri(baseUri, externalResource).ToString();
            }
        }
        else
        {
            element.Reference.ExternalResource = Path.GetFullPath(externalResource, _basePath);
        }

        var info = _references.GetOrAdd<T>(element.Reference.ExternalResource);
        info.References.Add(element);
    }

    public override void Visit(IOpenApiReferenceable referenceable)
    {
        switch (referenceable)
        {
            case OpenApiCallback callback:
                VisitReference(callback);
                break;
            case OpenApiExample example:
                VisitReference(example);
                break;
            case OpenApiHeader header:
                VisitReference(header);
                break;
            case OpenApiLink link:
                VisitReference(link);
                break;
            case OpenApiParameter parameter:
                VisitReference(parameter);
                break;
            case OpenApiPathItem pathItem:
                VisitReference(pathItem);
                break;
            case OpenApiRequestBody requestBody:
                VisitReference(requestBody);
                break;
            case OpenApiResponse response:
                VisitReference(response);
                break;
            case OpenApiSchema schema:
                VisitReference(schema);
                break;
            case OpenApiSecurityScheme securityScheme:
                VisitReference(securityScheme);
                break;
        }
    }
}