using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace RendleLabs.OpenApi.Bundle;

public abstract class ReferenceInfo
{
    protected ReferenceInfo(string path)
    {
        Path = path;
        if (path.StartsWith("http"))
        {
        }
        else
        {
            var id = System.IO.Path.GetFileName(path);
            var dot = id.IndexOf('.');
            if (dot > 0)
            {
                id = id[..dot];
            }

            Id = id;
        }
    }

    public string Path { get; }
    public string Id { get; set; }

    public abstract void Resolve(string text);
    
    public abstract ReferenceType Type { get; }
}

public class ReferenceInfo<T> : ReferenceInfo where T : IOpenApiReferenceable
{
    private static readonly Dictionary<Type, ReferenceType> ReferenceTypes = new Dictionary<Type, ReferenceType>
    {
        [typeof(OpenApiCallback)] = ReferenceType.Callback,
        [typeof(OpenApiExample)] = ReferenceType.Example,
        [typeof(OpenApiHeader)] = ReferenceType.Header,
        [typeof(OpenApiLink)] = ReferenceType.Link,
        [typeof(OpenApiParameter)] = ReferenceType.Parameter,
        [typeof(OpenApiRequestBody)] = ReferenceType.RequestBody,
        [typeof(OpenApiResponse)] = ReferenceType.Response,
        [typeof(OpenApiSchema)] = ReferenceType.Schema,
        [typeof(OpenApiSecurityScheme)] = ReferenceType.SecurityScheme,
        [typeof(OpenApiTag)] = ReferenceType.Tag,
    };

    public List<T> References { get; }

    public T ResolvedReference { get; set; }

    public override void Resolve(string text)
    {
        var reader = new OpenApiStringReader();
        ResolvedReference = reader.ReadFragment<T>(text, OpenApiSpecVersion.OpenApi3_0, out _);
    }

    public ReferenceInfo(string path) : base(path)
    {
        References = new List<T>();
    }

    public override ReferenceType Type => ReferenceTypes[typeof(T)];
}