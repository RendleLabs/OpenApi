using Microsoft.OpenApi.Interfaces;

namespace RendleLabs.OpenApi.Bundle;

public abstract class ReferenceInfo
{
    protected ReferenceInfo(string path)
    {
        Path = path;
    }

    public string Path { get; }
}

public class ReferenceInfo<T> : ReferenceInfo where T : IOpenApiReferenceable
{
    public List<T> References { get; }

    public ReferenceInfo(string path) : base(path)
    {
        References = new List<T>();
    }
}