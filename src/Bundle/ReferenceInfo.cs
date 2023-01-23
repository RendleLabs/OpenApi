using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace RendleLabs.OpenApi.Bundle;

public abstract class ReferenceInfo
{
    protected static readonly Dictionary<Type, ReferenceType> ReferenceTypes = new()
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

    protected ReferenceInfo(string path)
    {
        Path = path;
        ReadOnlySpan<char> id;
        if (ReferencePath.IsHttp(path))
        {
            var uri = new Uri(path);
            id = System.IO.Path.GetFileNameWithoutExtension(uri.AbsolutePath);
        }
        else
        {
            id = System.IO.Path.GetFileName(path);
        }

        id = id.TrimStart('.');
        
        var dot = id.IndexOf('.');
        if (dot > 0)
        {
            id = id[..dot];
        }

        Id = new string(id);
    }

    public string Path { get; }
    public string Id { get; set; }

    public abstract ReferenceType Type { get; }
}

public class ReferenceInfo<T> : ReferenceInfo where T : IOpenApiReferenceable
{
    public List<T> References { get; }

    public T? ResolvedReference { get; set; }

    public ReferenceInfo(string path) : base(path)
    {
        References = new List<T>();
    }

    public override ReferenceType Type => ReferenceTypes[typeof(T)];
}