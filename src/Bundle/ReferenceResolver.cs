using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Bundle;

public class ReferenceResolver
{
    private readonly OpenApiDocument _document;
    private readonly ReferenceInfoCollection _references;

    public ReferenceResolver(OpenApiDocument document, ReferenceInfoCollection references)
    {
        _document = document;
        _references = references;
    }

    public async Task ResolveAsync()
    {
        await LoadAllReferencesAsync();

        foreach (var reference in _references)
        {
            switch (reference)
            {
                case ReferenceInfo<OpenApiCallback> callback:
                    Resolve(callback);
                    break;
                case ReferenceInfo<OpenApiExample> example:
                    Resolve(example);
                    break;
                case ReferenceInfo<OpenApiHeader> header:
                    Resolve(header);
                    break;
                case ReferenceInfo<OpenApiLink> link:
                    Resolve(link);
                    break;
                case ReferenceInfo<OpenApiParameter> parameter:
                    Resolve(parameter);
                    break;
                case ReferenceInfo<OpenApiRequestBody> requestBody:
                    Resolve(requestBody);
                    break;
                case ReferenceInfo<OpenApiResponse> response:
                    Resolve(response);
                    break;
                case ReferenceInfo<OpenApiSchema> schema:
                    Resolve(schema);
                    break;
                case ReferenceInfo<OpenApiSecurityScheme> securityScheme:
                    Resolve(securityScheme);
                    break;
                case ReferenceInfo<OpenApiTag> tag:
                    break;
            }
        }
    }

    private void Resolve<T>(ReferenceInfo<T> referenceInfo) where T : class, IOpenApiReferenceable
    {
        var collection = EnsureCollection<T>(referenceInfo.Type);
        if (collection is null) return;
        collection[referenceInfo.Id] = referenceInfo.ResolvedReference;
        foreach (var reference in referenceInfo.References)
        {
            reference.Reference = new OpenApiReference
            {
                Id = referenceInfo.Id,
                Type = referenceInfo.Type
            };
        }
    }

    private Dictionary<string, T>? EnsureCollection<T>(ReferenceType type) where T : IOpenApiReferenceable
    {
        _document.Components ??= new OpenApiComponents();

        object? collection = type switch
        {
            ReferenceType.Callback => _document.Components.Callbacks ??= new Dictionary<string, OpenApiCallback>(),
            ReferenceType.Example => _document.Components.Examples ??= new Dictionary<string, OpenApiExample>(),
            ReferenceType.Header => _document.Components.Headers ??= new Dictionary<string, OpenApiHeader>(),
            ReferenceType.Link => _document.Components.Links ??= new Dictionary<string, OpenApiLink>(),
            ReferenceType.Parameter => _document.Components.Parameters ??= new Dictionary<string, OpenApiParameter>(),
            ReferenceType.RequestBody => _document.Components.RequestBodies ??= new Dictionary<string, OpenApiRequestBody>(),
            ReferenceType.Response => _document.Components.Responses ??= new Dictionary<string, OpenApiResponse>(),
            ReferenceType.Schema => _document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>(),
            ReferenceType.SecurityScheme => _document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>(),
            _ => null
        };

        return collection as Dictionary<string, T>;
    }

    private async Task LoadAllReferencesAsync()
    {
        using var loader = new ReferenceLoader();
        
        while (true)
        {
            int count = _references.Count;

            foreach (var reference in _references.ToArray())
            {
                var d = new OpenApiDocument();
                d.Components ??= new OpenApiComponents();
                switch (reference)
                {
                    case ReferenceInfo<OpenApiCallback> callbackInfo:
                        var callback = await loader.LoadAsync(callbackInfo);
                        d.Components.Callbacks ??= new Dictionary<string, OpenApiCallback>();
                        d.Components.Callbacks[reference.Id] = callbackInfo.ResolvedReference = callback;
                        break;
                    case ReferenceInfo<OpenApiExample> exampleInfo:
                        var example = await loader.LoadAsync(exampleInfo);
                        d.Components.Examples ??= new Dictionary<string, OpenApiExample>();
                        d.Components.Examples[reference.Id] = exampleInfo.ResolvedReference = example;
                        break;
                    case ReferenceInfo<OpenApiHeader> headerInfo:
                        var header = await loader.LoadAsync(headerInfo);
                        d.Components.Headers ??= new Dictionary<string, OpenApiHeader>();
                        d.Components.Headers[reference.Id] = headerInfo.ResolvedReference = header;
                        break;
                    case ReferenceInfo<OpenApiLink> linkInfo:
                        var link = await loader.LoadAsync(linkInfo);
                        d.Components.Links ??= new Dictionary<string, OpenApiLink>();
                        d.Components.Links[reference.Id] = linkInfo.ResolvedReference = link;
                        break;
                    case ReferenceInfo<OpenApiParameter> parameterInfo:
                        var parameter = await loader.LoadAsync(parameterInfo);
                        d.Components.Parameters ??= new Dictionary<string, OpenApiParameter>();
                        d.Components.Parameters[reference.Id] = parameterInfo.ResolvedReference = parameter;
                        break;
                    case ReferenceInfo<OpenApiRequestBody> requestBodyInfo:
                        var requestBody = await loader.LoadAsync(requestBodyInfo);
                        d.Components.RequestBodies ??= new Dictionary<string, OpenApiRequestBody>();
                        d.Components.RequestBodies[reference.Id] = requestBodyInfo.ResolvedReference = requestBody;
                        break;
                    case ReferenceInfo<OpenApiResponse> responseInfo:
                        var response = await loader.LoadAsync(responseInfo);
                        d.Components.Responses ??= new Dictionary<string, OpenApiResponse>();
                        d.Components.Responses[reference.Id] = responseInfo.ResolvedReference = response;
                        break;
                    case ReferenceInfo<OpenApiSchema> schemaInfo:
                        var schema = await loader.LoadAsync(schemaInfo);
                        d.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();
                        d.Components.Schemas[reference.Id] = schemaInfo.ResolvedReference = schema;
                        break;
                    case ReferenceInfo<OpenApiSecurityScheme> securitySchemeInfo:
                        var securityScheme = await loader.LoadAsync(securitySchemeInfo);
                        d.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                        d.Components.SecuritySchemes[reference.Id] = securitySchemeInfo.ResolvedReference = securityScheme;
                        break;
                    case ReferenceInfo<OpenApiTag> tag:
                        break;
                }
            }

            if (_references.Count == count) break;
        }
    }
}